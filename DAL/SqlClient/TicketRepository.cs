using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SC.BL.Domain;

namespace SC.DAL.SqlClient
{
  public class TicketRepository : ITicketRepository
  {
    private SqlConnection GetConnection()
    {
      string connectionString = ConfigurationManager.ConnectionStrings["SupportCenterDB_SqlClient"].ConnectionString;
      return new SqlConnection(connectionString);
    }

    public IEnumerable<Ticket> ReadTickets()
    {
      List<Ticket> tickets = new List<Ticket>();

      string selectStatement = "SELECT TicketNumber, AccountId, [Text], DateOpened, State, DeviceName FROM Ticket";

      using (var connection = this.GetConnection())
      {
        SqlCommand command = new SqlCommand(selectStatement, connection);

        connection.Open();

        using (SqlDataReader reader = command.ExecuteReader())
        {
          int ticketNumberOrdinal = reader.GetOrdinal("TicketNumber");
          int accountIdOrdinal = reader.GetOrdinal("AccountId");
          int textOrdinal = reader.GetOrdinal("Text");
          int dateOpenedOrdinal = reader.GetOrdinal("DateOpened");
          int stateOrdinal = reader.GetOrdinal("State");
          int deviceNameOrdinal = reader.GetOrdinal("DeviceName");

          while (reader.Read())
          {
            Ticket ticket;
            string deviceName = reader.IsDBNull(deviceNameOrdinal) ? null : reader.GetString(deviceNameOrdinal);
            if (deviceName == null)
              ticket = new Ticket();
            else
              ticket = new HardwareTicket() { DeviceName = deviceName };

            ticket.TicketNumber = reader.GetInt32(ticketNumberOrdinal);
            ticket.AccountId = reader.GetInt32(accountIdOrdinal);
            ticket.Text = reader.GetString(textOrdinal);
            ticket.DateOpened = reader.GetDateTime(dateOpenedOrdinal);
            ticket.State = (TicketState)reader.GetByte(stateOrdinal);

            tickets.Add(ticket);
          }
          reader.Close(); // good practice!
        }
        connection.Close(); // good practice!
      }
      return tickets;
    }

    public Ticket ReadTicket(int ticketNumber)
    {
      Ticket requestedTicket = null;

      string selectStatement = "SELECT TicketNumber, AccountId, [Text], DateOpened, State, DeviceName FROM Ticket"
                               + " WHERE TicketNumber = @ticketNumber";

      using (var connection = this.GetConnection())
      {
        SqlCommand command = new SqlCommand(selectStatement, connection);
        command.Parameters.AddWithValue("@ticketNumber", ticketNumber);

        connection.Open();
        using (SqlDataReader reader = command.ExecuteReader())
        {
          int ticketNumberOrdinal = reader.GetOrdinal("TicketNumber");
          int accountIdOrdinal = reader.GetOrdinal("AccountId");
          int textOrdinal = reader.GetOrdinal("Text");
          int dateOpenedOrdinal = reader.GetOrdinal("DateOpened");
          int stateOrdinal = reader.GetOrdinal("State");
          int deviceNameOrdinal = reader.GetOrdinal("DeviceName");

          if (reader.Read())
          {
            string deviceName = reader.IsDBNull(deviceNameOrdinal) ? null : reader.GetString(deviceNameOrdinal);
            if (deviceName == null)
              requestedTicket = new Ticket();
            else
              requestedTicket = new HardwareTicket() { DeviceName = deviceName };

            requestedTicket.TicketNumber = reader.GetInt32(ticketNumberOrdinal);
            requestedTicket.AccountId = reader.GetInt32(accountIdOrdinal);
            requestedTicket.Text = reader.GetString(textOrdinal);
            requestedTicket.DateOpened = reader.GetDateTime(dateOpenedOrdinal);
            requestedTicket.State = (TicketState)reader.GetByte(stateOrdinal);
          }
          reader.Close(); // good practice!
        }
        connection.Close(); // good practice!
      }
      return requestedTicket;
    }

    public Ticket CreateTicket(Ticket ticket)
    {
      string insertStatement = "INSERT INTO Ticket(AccountId, [Text], DateOpened, State, DeviceName)"
                               + " VALUES (@accountId, @text, @dateOpened, @state, @deviceName)";

      using (var connection = this.GetConnection())
      {
        SqlCommand command = new SqlCommand(insertStatement, connection);
        command.Parameters.AddWithValue("@accountId", ticket.AccountId);
        command.Parameters.AddWithValue("@text", ticket.Text);
        command.Parameters.AddWithValue("@dateOpened", ticket.DateOpened.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@state", (byte)ticket.State);

        if (ticket is HardwareTicket)
          command.Parameters.AddWithValue("@deviceName", ((HardwareTicket)ticket).DeviceName);
        else
          command.Parameters.AddWithValue("@deviceName", DBNull.Value);

        // Retrieve primary key 'TicketNumber' of inserted ticket
        command.CommandText += "; SELECT SCOPE_IDENTITY();";

        connection.Open();
        ticket.TicketNumber = Convert.ToInt32(command.ExecuteScalar());
        connection.Close(); // good practice!
      }
      return ticket;
    }

    public void UpdateTicket(Ticket ticket)
    {
      string updateStatement = "UPDATE Ticket SET AccountId = @accountId, [Text] = @text"
                               + ", DateOpened = @dateOpened, State = @state" + ", DeviceName = @deviceName"
                               + " WHERE TicketNumber = @ticketNumber";

      using (var connection = this.GetConnection())
      {
        SqlCommand command = new SqlCommand(updateStatement, connection);
        command.Parameters.AddWithValue("@accountId", ticket.AccountId);
        command.Parameters.AddWithValue("@text", ticket.Text);
        command.Parameters.AddWithValue("@dateOpened", ticket.DateOpened.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@state", (byte)ticket.State);
        command.Parameters.AddWithValue("@ticketNumber", ticket.TicketNumber);
        if (ticket is HardwareTicket)
          command.Parameters.AddWithValue("@deviceName", ((HardwareTicket)ticket).DeviceName);
        else
          command.Parameters.AddWithValue("@deviceName", DBNull.Value);
        

        connection.Open();
        int rowsAffected = command.ExecuteNonQuery();
        connection.Close(); // good practice!
      }
    }

    public void DeleteTicket(int ticketNumber)
    {
      string deleteTicketStatement = "DELETE FROM Ticket WHERE TicketNumber = @ticketNumber";
      string deleteResponsesOfTicketStatement = "DELETE FROM TicketResponse" + " WHERE Ticket_TicketNumber = @ticketNumber";

      using (var connection = this.GetConnection())
      {
        var ticketCmd = new SqlCommand(deleteTicketStatement, connection);
        ticketCmd.Parameters.AddWithValue("@ticketNumber", ticketNumber);

        var responsesCmd = new SqlCommand(deleteResponsesOfTicketStatement, connection);
        responsesCmd.Parameters.AddWithValue("@ticketNumber", ticketNumber);

        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
          responsesCmd.Transaction = transaction;
          ticketCmd.Transaction = transaction;

          responsesCmd.ExecuteNonQuery();
          ticketCmd.ExecuteNonQuery();

          transaction.Commit();
        }
        connection.Close();
      }
    }

    public IEnumerable<TicketResponse> ReadTicketResponsesOfTicket(int ticketNumber)
    {
      List<TicketResponse> requestedTicketResponses = new List<TicketResponse>();

      string sql = "SELECT TicketResponse.Id AS rId, TicketResponse.[Text] AS rText, [Date], IsClientResponse FROM TicketResponse"
                   + " INNER JOIN Ticket" + " ON Ticket.TicketNumber = TicketResponse.Ticket_TicketNumber"
                   + " WHERE Ticket.TicketNumber = @ticketNumber";

      using (var connection = this.GetConnection())
      {
        SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ticketNumber", ticketNumber);

        connection.Open();

        using (SqlDataReader reader = command.ExecuteReader())
        {
          int idOrdinal = reader.GetOrdinal("rId");
          int responseOrdinal = reader.GetOrdinal("rText");
          int dateOrdinal = reader.GetOrdinal("Date");
          int isClientOrdinal = reader.GetOrdinal("IsClientResponse");
          while (reader.Read())
          {
            requestedTicketResponses.Add(new TicketResponse()
            {
              Id = reader.GetInt32(idOrdinal),
              Text = reader.GetString(responseOrdinal),
              Date = reader.GetDateTime(dateOrdinal),
              IsClientResponse = reader.GetBoolean(isClientOrdinal)
            });
          }
          reader.Close(); // good practice!
        }
        connection.Close(); // good practice!
      }
      return requestedTicketResponses;
    }

    public TicketResponse CreateTicketResponse(TicketResponse response)
    {
      if (response.Ticket != null)
      {
        string insertStatement = "INSERT INTO TicketResponse([Text], [Date], IsClientResponse, Ticket_TicketNumber)"
                                 + " VALUES (@text, @date, @isClientResponse, @tickedNumber)";

        using (var connection = this.GetConnection())
        {
          SqlCommand command = new SqlCommand(insertStatement, connection);
          command.Parameters.AddWithValue("@text", response.Text);
          command.Parameters.AddWithValue("@date", response.Date.ToString("yyyy-MM-dd HH:mm:ss"));
          command.Parameters.AddWithValue("@isClientResponse", response.IsClientResponse);
          command.Parameters.AddWithValue("@tickedNumber", response.Ticket.TicketNumber);

          //Retrieve primary key 'Id' of inserted response
          command.CommandText += "; SELECT SCOPE_IDENTITY();";

          connection.Open();
          response.Id = Convert.ToInt32(command.ExecuteScalar());
          connection.Close(); // good practice!
        }
        return response;
      }
      else
        throw new ArgumentException("The ticketresponse has no ticket attached to it");
    }

    public void UpdateTicketStateToClosed(int ticketNumber)
    {
      using (var connection = this.GetConnection())
      {
        SqlCommand command = new SqlCommand("sp_CloseTicket", connection);
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@ticketNumber", ticketNumber);

        connection.Open();
        int rowsAffected = command.ExecuteNonQuery();
        connection.Close(); // good practice!
      }
    }
  }
}
