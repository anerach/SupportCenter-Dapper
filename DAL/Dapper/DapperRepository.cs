using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Dapper;
using SC.BL.Domain;

namespace SC.DAL.Dapper
{
    public class DapperRepository : ITicketRepository
    {
        private SqlConnection GetConnection()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["SupportCenterDB_Dapper"].ConnectionString;
            return new SqlConnection(connectionString);
        }

        private delegate void OneToManyDelegate(object parent, object child);

        private IEnumerable<TParent> OneToMany<TParent, TChild>(string sql, OneToManyDelegate oneToMany, object data = null)
        {
            var lookup = new Dictionary<int, TParent>();

            using (var conn = GetConnection())
            {
                conn.Open();

                conn.Query<TParent, TChild, TParent>(sql, (p, c) =>
                {
                    TParent parent;

                    if (!lookup.TryGetValue(p.GetHashCode(), out parent))
                    {
                        lookup.Add(p.GetHashCode(), parent = p);
                    }

                    oneToMany(parent, c);

                    return parent;
                }, data);

                conn.Close();
            }

            return lookup.Values.ToArray();
        }

        private void TicketAndTicketResponseImplementation(object parent, object child)
        {
            var ticket = (Ticket)parent;
            var response = (TicketResponse)child;

            if (ticket.Responses == null)
                ticket.Responses = new List<TicketResponse>();

            ticket.Responses.Add(response);
        }

        private void HardwareTicketAndTicketResponseImplemention(object parent, object child)
        {
            var ticket = (HardwareTicket)parent;
            var response = (TicketResponse)child;

            if (ticket.Responses == null)
                ticket.Responses = new List<TicketResponse>();

            ticket.Responses.Add(response);
        }

        public IEnumerable<Ticket> ReadTickets()
        {
            var sql = "SELECT Ticket.*, TicketResponse.* FROM [Ticket] LEFT JOIN [TicketResponse] ON TicketResponse.Ticket_TicketNumber = Ticket.TicketNumber";

            return OneToMany<Ticket, TicketResponse>(sql, TicketAndTicketResponseImplementation);
        }

        public Ticket ReadTicket(int ticketNumber)
        {
            Ticket ticket;

            using (var conn = GetConnection())
            {
                conn.Open();

                var result = conn.Query<string>("SELECT DeviceName AS Result FROM Ticket WHERE TicketNumber = @ticketNumber AND DeviceName IS NOT NULL", new { ticketNumber = ticketNumber }).Count();

                if (result == 0)
                {   //  Reg Ticket
                    var sql = "SELECT Ticket.*, TicketResponse.* FROM [Ticket] LEFT JOIN [TicketResponse] ON TicketResponse.Ticket_TicketNumber = Ticket.TicketNumber WHERE Ticket.TicketNumber = @ticketNumber";

                    ticket = OneToMany<Ticket, TicketResponse>(sql, TicketAndTicketResponseImplementation, new { ticketNumber = ticketNumber }).Single();
                }
                else
                {   // Hardware Ticket
                    var sql = "SELECT Ticket.*, TicketResponse.* FROM [Ticket] LEFT JOIN [TicketResponse] ON TicketResponse.Ticket_TicketNumber = Ticket.TicketNumber WHERE Ticket.TicketNumber = @ticketNumber";

                    ticket = OneToMany<HardwareTicket, TicketResponse>(sql, HardwareTicketAndTicketResponseImplemention, new { ticketNumber = ticketNumber }).Single();
                }

                conn.Close();
            }

            return ticket;
        }

        public Ticket CreateTicket(Ticket ticket)
        {
            var sql = "INSERT INTO Ticket(AccountId, [Text], DateOpened, State, DeviceName) VALUES (@accountId, @text, @dateOpened, @state, @deviceName); SELECT Cast (Scope_Identity() as int);";

            object insertData = new
            {
                accountId = ticket.AccountId,
                text = ticket.Text,
                dateOpened = ticket.DateOpened,
                state = ticket.State,
                deviceName = (ticket as HardwareTicket)?.DeviceName
            };

            using (var conn = GetConnection())
            {
                conn.Open();
                ticket.TicketNumber = conn.Query<int>(sql, insertData).Single();
                conn.Close();
            }
            return ticket;
        }

        public void UpdateTicket(Ticket ticket)
        {
            var sql = "UPDATE Ticket SET AccountId = @accountId, [Text] = @text"
                         + ", DateOpened = @dateOpened, State = @state" + ", DeviceName = @deviceName"
                         + " WHERE TicketNumber = @ticketNumber";
            object toReplace;

            if (ticket is HardwareTicket)
            {
                toReplace = new
                {
                    accountId = ticket.AccountId,
                    text = ticket.Text,
                    dateOpened = ticket.DateOpened,
                    state = ticket.State,
                    deviceName = ((HardwareTicket)ticket).DeviceName, // Null or other if typeof()... Fix at home //Situering in de rest
                    ticketNumber = ticket.TicketNumber
                };
            }
            else
            {
                toReplace = new
                {
                    accountId = ticket.AccountId,
                    text = ticket.Text,
                    dateOpened = ticket.DateOpened,
                    state = ticket.State, // Null or other if typeof()... Fix at home //Situering in de rest
                    ticketNumber = ticket.TicketNumber
                };
            }


            using (var conn = GetConnection())
            {
                conn.Open();
                conn.Execute(sql, toReplace);
                conn.Close();
            }
        }

        public void UpdateTicketStateToClosed(int ticketNumber)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                conn.Execute("sp_CloseTicket", new { ticketNumber = ticketNumber }, commandType: CommandType.StoredProcedure);
                conn.Close();
            }
        }

        public void DeleteTicket(int ticketNumber)
        {
            var deleteTicketStatement = "DELETE FROM Ticket WHERE TicketNumber = @ticketNumber";
            var deleteResponsesOfTicketStatement = "DELETE FROM TicketResponse" + " WHERE Ticket_TicketNumber = @ticketNumber";

            var sql = deleteTicketStatement + ";" + deleteResponsesOfTicketStatement;

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var tranScope = new TransactionScope())
                {
                    conn.Execute(sql, new { ticketNumber = ticketNumber });
                    tranScope.Complete();
                }
                conn.Close();
            }
        }

        private IEnumerable<TicketResponse> ReadTicketResponsesOfTicket(int ticketNumber, SqlConnection conn)
        {
            var sql = "SELECT TicketResponse.Id AS Id, TicketResponse.[Text] AS Text, [Date], IsClientResponse FROM TicketResponse LEFT JOIN Ticket ON Ticket.TicketNumber = TicketResponse.Ticket_TicketNumber WHERE Ticket.TicketNumber = @ticketNumber";

            /*
            Dapper mapping to a different columnname
            ie in db it's first_name but in object it's firstName
            SELECT first_name as firstName...
            Then maps automatically
            */

            return conn.Query<TicketResponse>(sql, new { ticketNumber = ticketNumber });
        }

        public IEnumerable<TicketResponse> ReadTicketResponsesOfTicket(int ticketNumber)
        {
            IEnumerable<TicketResponse> responses;

            using (var conn = GetConnection())
            {
                conn.Open();

                responses = ReadTicketResponsesOfTicket(ticketNumber, conn);

                conn.Close();
            }

            return responses;
        }

        public TicketResponse CreateTicketResponse(TicketResponse response)
        {
            var sql = "INSERT INTO TicketResponse([Text], [Date], IsClientResponse, Ticket_TicketNumber) VALUES (@text, @date, @isClientResponse, @ticketNumber);SELECT SCOPE_IDENTITY();";

            object toReplace = new
            {
                text = response.Text,
                date = response.Date,
                isClientResponse = response.IsClientResponse,
                ticketNumber = response.Ticket.TicketNumber
            };

            using (var conn = GetConnection())
            {
                conn.Open();
                response.Id = (int)conn.Query<decimal>(sql, toReplace).Single();
                conn.Close();
            }

            return response;
        }
    }
}