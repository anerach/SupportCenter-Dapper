using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using SC.BL.Domain;

namespace SC.DAL.Dapper
{
    public class DapperContribRepository : ITicketRepository
    {
        private SqlConnection GetConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["SupportCenterDB_Dapper"].ConnectionString;
            return new SqlConnection(connectionString);
        }

        public IEnumerable<Ticket> ReadTickets()
        {
            IEnumerable<Ticket> tickets;

            using (var conn = GetConnection())
            {
                conn.Open();
                tickets = conn.GetAll<Ticket>();
                conn.Close();
            }

            return tickets;
            // throw new NotImplementedException();
        }

        public Ticket CreateTicket(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public Ticket ReadTicket(int ticketNumber)
        {
            throw new NotImplementedException();
        }

        public void UpdateTicket(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public void UpdateTicketStateToClosed(int ticketNumber)
        {
            throw new NotImplementedException();
        }

        public void DeleteTicket(int ticketNumber)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TicketResponse> ReadTicketResponsesOfTicket(int ticketNumber)
        {
            IEnumerable<TicketResponse> tickets;

            using (var conn = GetConnection())
            {
                conn.Open();
                tickets = conn.GetAll<TicketResponse>().Where(t => t.Ticket.TicketNumber == ticketNumber);
                conn.Close();
            }
            return tickets;
            //throw new NotImplementedException();
        }

        public TicketResponse CreateTicketResponse(TicketResponse response)
        {
            throw new NotImplementedException();
        }
    }
}
