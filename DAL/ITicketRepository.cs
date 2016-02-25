using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SC.BL.Domain;

namespace SC.DAL
{
  public interface ITicketRepository
  {
    #region Ticket
    IEnumerable<Ticket> ReadTickets();
    // CRUD Ticket
    Ticket CreateTicket(Ticket ticket);
    Ticket ReadTicket(int ticketNumber);
    void UpdateTicket(Ticket ticket);
    void UpdateTicketStateToClosed(int ticketNumber);
    void DeleteTicket(int ticketNumber);
    #endregion

    #region TicketResponse
    IEnumerable<TicketResponse> ReadTicketResponsesOfTicket(int ticketNumber);
    //// CRUD TicketResponse
    TicketResponse CreateTicketResponse(TicketResponse response);
    //TicketResponse ReadTicketResponse(int id);
    //void UpdateTicketResponse(TicketResponse response);
    //void DeleteTicketResponse(int id);
    #endregion
  }
}
