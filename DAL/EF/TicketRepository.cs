using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SC.BL.Domain;

namespace SC.DAL.EF
{
  public class TicketRepository : ITicketRepository
  {
    private SupportCenterDbContext ctx;

    public TicketRepository()
    {
      ctx = new SupportCenterDbContext();
      
      ctx.Database.Initialize(false);
    }

    public IEnumerable<Ticket> ReadTickets()
    {
      //IEnumerable<Ticket> tickets = ctx.Tickets.AsEnumerable<Ticket>();

      // Eager-loading
      //IEnumerable<Ticket> tickets = ctx.Tickets.Include(t => t.Responses).AsEnumerable<Ticket>();

      // Lazy-loading
      //IEnumerable<Ticket> tickets = ctx.Tickets.AsEnumerable<Ticket>(); // needs 'Multiple Active Result Sets' (MARS) for lazy-loading (connectionstring)
      IEnumerable<Ticket> tickets = ctx.Tickets.ToList<Ticket>(); // all (parent-)entities are loaded before lazy-loading associated data (doesn't need MARS)

      return tickets;
    }

    public Ticket ReadTicket(int ticketNumber)
    {
      Ticket ticket = ctx.Tickets.Find(ticketNumber);
      return ticket;
    }

    public Ticket CreateTicket(Ticket ticket)
    {
      ctx.Tickets.Add(ticket);
      ctx.SaveChanges();

      return ticket; // 'TicketNumber' has been created by the database!
    }

    public void UpdateTicket(Ticket ticket)
    {
      // Make sure that 'ticket' is known by context
      // and has state 'Modified' before updating to database
      ctx.Entry(ticket).State = System.Data.Entity.EntityState.Modified;
      ctx.SaveChanges();
    }

    public void UpdateTicketStateToClosed(int ticketNumber)
    {
      Ticket ticket = ctx.Tickets.Find(ticketNumber);
      ticket.State = TicketState.Closed;
      ctx.SaveChanges();
    }

    public void DeleteTicket(int ticketNumber)
    {
      Ticket ticket = ctx.Tickets.Find(ticketNumber);
      ctx.Tickets.Remove(ticket);
      ctx.SaveChanges();
    }

    public IEnumerable<TicketResponse> ReadTicketResponsesOfTicket(int ticketNumber)
    {
      IEnumerable<TicketResponse> responses = ctx.TicketResponses.Where(r => r.Ticket.TicketNumber == ticketNumber).AsEnumerable();
      return responses;
    }

    public TicketResponse CreateTicketResponse(TicketResponse response)
    {
      ctx.TicketResponses.Add(response);
      ctx.SaveChanges();

      return response; // 'Id' has been created by the database!
    }
  }
}
