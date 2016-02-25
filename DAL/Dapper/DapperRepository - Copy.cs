using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions; //Gotta reference this
using Dapper;
using SC.BL.Domain;

namespace SC.DAL.Dapper
{
    public class DapperRepository2 : ITicketRepository
    {
        private SqlConnection GetConnection()
        {
            //string connectionString = ConfigurationManager.ConnectionStrings["SupportCenterDB_AZ"].ConnectionString;
            string connectionString = ConfigurationManager.ConnectionStrings["SupportCenterDB_Dapper"].ConnectionString;
            return new SqlConnection(connectionString);
        }

        public IEnumerable<Ticket> ReadTickets()
        {
            IEnumerable<Ticket> tickets;
            using (var conn = GetConnection())
            {
                conn.Open();

                string query = "SELECT TicketNumber, AccountId, [Text], DateOpened, State, DeviceName FROM Ticket";

                string oldQuery = "SELECT TicketNumber, AccountId, [Text], DateOpened, State, DeviceName FROM Ticket";

                tickets =
                    conn.Query<Ticket>(query);
                conn.Close();
            }
            return tickets;

            //throw new System.NotImplementedException();
        }

        public Ticket CreateTicket(Ticket ticket)
        {
            Ticket returnTicket;
            string sql = "INSERT INTO Ticket(AccountId, [Text], DateOpened, State, DeviceName)"
                         + " VALUES (@accountId, @text, @dateOpened, @state, @deviceName);SELECT SCOPE_IDENTITY();";

            object insertData = new
            {
                accountId = ticket.AccountId,
                text = ticket.Text,
                dateOpened = ticket.DateOpened,
                state = ticket.State,
                deviceName = (ticket is HardwareTicket) ? ((HardwareTicket)ticket).DeviceName : (string)null
            };

            using (var conn = GetConnection())
            {
                conn.Open();
                ticket.TicketNumber = (int)conn.Query<decimal>(sql, insertData).Single(); //Scopeidentity returns a double?
                conn.Close();
            }
            return ticket;
        }

        public Ticket ReadTicket(int ticketNumber)
        {
            Ticket ticket;
            HardwareTicket hardTicket;  
            using (var conn = GetConnection())
            {
                conn.Open();
                hardTicket =
                    conn.Query<HardwareTicket>(
                        "SELECT TicketNumber, AccountId, [Text], DateOpened, State, DeviceName FROM Ticket WHERE TicketNumber = @ticketNumber",
                        new { ticketNumber = ticketNumber }).Single();

                conn.Close();
            }

            if (hardTicket.DeviceName == null)
            {
                ticket = (Ticket)hardTicket;
                Console.WriteLine(ticket.GetType());
                Console.WriteLine("Is actually regular ticket...");
            }
            else
            {
                ticket = hardTicket;
            }
            return ticket;
        }

        public void UpdateTicket(Ticket ticket)
        {
            string sql = "UPDATE Ticket SET AccountId = @accountId, [Text] = @text"
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
            //throw new System.NotImplementedException();
        }

        public void DeleteTicket(int ticketNumber)
        {
            string deleteTicketStatement = "DELETE FROM Ticket WHERE TicketNumber = @ticketNumber";
            string deleteResponsesOfTicketStatement = "DELETE FROM TicketResponse" + " WHERE Ticket_TicketNumber = @ticketNumber";

            string sql = deleteTicketStatement + ";" + deleteResponsesOfTicketStatement;
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

            //            throw new System.NotImplementedException();
        }

        public IEnumerable<TicketResponse> ReadTicketResponsesOfTicket(int ticketNumber)
        {
            IEnumerable<TicketResponse> requestedTicketResponses = new List<TicketResponse>();

            string sql = "SELECT TicketResponse.Id AS Id, TicketResponse.[Text] AS Text, [Date], IsClientResponse FROM TicketResponse"
                         + " INNER JOIN Ticket" + " ON Ticket.TicketNumber = TicketResponse.Ticket_TicketNumber"
                         + " WHERE Ticket.TicketNumber = @ticketNumber";

            /*
            Dapper mapping to a different columnname
            ie in db it's first_name but in object it's firstName
            SELECT first_name as firstName...
            Then maps automatically
            */

            using (var conn = GetConnection())
            {
                conn.Open();
                requestedTicketResponses =
                    conn.Query<TicketResponse>(sql, new { ticketNumber = ticketNumber });
                conn.Close();
            }

            return requestedTicketResponses;
        }

        public TicketResponse CreateTicketResponse(TicketResponse response)
        {
            string sql = "INSERT INTO TicketResponse([Text], [Date], IsClientResponse, Ticket_TicketNumber)"
                         + " VALUES (@text, @date, @isClientResponse, @ticketNumber);SELECT SCOPE_IDENTITY();";

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
            //throw new System.NotImplementedException();
        }
    }
}