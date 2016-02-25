using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SC.BL.Domain;

namespace SC.DAL
{
  public class TicketRepositoryHC : ITicketRepository
  {
    private List<Ticket> tickets;
    private List<TicketResponse> responses;

    public TicketRepositoryHC()
    {
      Seed();
    }

    private void Seed()
    {
      tickets = new List<Ticket>();
      responses = new List<TicketResponse>();

      // Aanmaken eerste ticket met drie responses
      Ticket t1 = new Ticket()
      {
        TicketNumber = tickets.Count + 1,
        AccountId = 1,
        Text = "Ik kan mij niet aanmelden op de webmail",
        DateOpened = new DateTime(2012, 9, 9, 13, 5, 59),
        State = TicketState.Closed,
        Responses = new List<TicketResponse>()
      };

      tickets.Add(t1);

      TicketResponse t1r1 = new TicketResponse()
      {
        Id = responses.Count + 1,
        Ticket = t1,
        Text = "Account is geblokkeerd",
        Date = new DateTime(2012, 9, 9, 13, 24, 48),
        IsClientResponse = false
      };
      t1.Responses.Add(t1r1);
      responses.Add(t1r1);

      TicketResponse t1r2 = new TicketResponse()
      {
        Id = responses.Count + 1,
        Ticket = t1,
        Text = "Account terug in orde en nieuw paswoord ingesteld",
        Date = new DateTime(2012, 9, 9, 13, 29, 11),
        IsClientResponse = false
      };
      t1.Responses.Add(t1r2);
      responses.Add(t1r2);

      TicketResponse t1r3 = new TicketResponse()
      {
        Id = responses.Count + 1,
        Ticket = t1,
        Text = "Aanmelden gelukt en paswoord gewijzigd",
        Date = new DateTime(2012, 9, 10, 7, 22, 36),
        IsClientResponse = true
      };
      t1.Responses.Add(t1r3);
      responses.Add(t1r3);

      t1.State = TicketState.Closed;


      //Aanmaken tweede ticket met één response
      Ticket t2 = new Ticket()
      {
        TicketNumber = tickets.Count + 1,
        AccountId = 1,
        Text = "Geen internetverbinding",
        DateOpened = new DateTime(2012, 11, 5, 9, 45, 13),
        State = TicketState.Open,
        Responses = new List<TicketResponse>()
      };

      tickets.Add(t2);

      TicketResponse t2r1 = new TicketResponse()
      {
        Id = responses.Count + 1,
        Ticket = t2,
        Text = "Controleer of de kabel goed is aangesloten",
        Date = new DateTime(2012, 11, 5, 11, 25, 42),
        IsClientResponse = false
      };
      t2.Responses.Add(t2r1);
      responses.Add(t2r1);

      t2.State = TicketState.Answered;

      //Aanmaken eerste HardwareTicket
      HardwareTicket ht1 = new HardwareTicket()
      {
        TicketNumber = tickets.Count + 1,
        AccountId = 2,
        Text = "Blue screen!",
        DateOpened = new DateTime(2012, 12, 14, 19, 15, 32),
        State = TicketState.Open,
        //Responses = new List<TicketResponse>(),
        DeviceName = "PC-123456"
      };

      tickets.Add(ht1);
    }

    #region 'ITicketRepository' implementatie
    public Ticket CreateTicket(Ticket ticket)
    {
      ticket.TicketNumber = tickets.Count + 1;
      tickets.Add(ticket);

      return ticket;
    }

    public IEnumerable<Ticket> ReadTickets()
    {
      return tickets;
    }

    public Ticket ReadTicket(int ticketNumber)
    {
      return tickets.Find(t => t.TicketNumber == ticketNumber);
    }

    public void UpdateTicket(Ticket ticket)
    {
      // Do nothing! All data lives in memory, everything references the same objects!!
    }

    public void DeleteTicket(int ticketNumber)
    {
      this.responses.RemoveAll(r => r.Ticket.TicketNumber == ticketNumber);
      this.tickets.Remove(ReadTicket(ticketNumber));
    }

    public IEnumerable<TicketResponse> ReadTicketResponsesOfTicket(int ticketNumber)
    {
      return tickets.Find(t => t.TicketNumber == ticketNumber).Responses;
    }

    public TicketResponse CreateTicketResponse(TicketResponse response)
    {
      response.Id = responses.Count + 1;
      responses.Add(response);
      return response;
    }

    public void UpdateTicketStateToClosed(int ticketNumber)
    {
      // Do nothing! All data lives in memory, everything references the same objects!!
    }
    #endregion
  }
}
