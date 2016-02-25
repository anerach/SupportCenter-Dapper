using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SC.BL.Domain;

namespace SC.BL
{
  public interface ITicketManager
  {
    IEnumerable<Ticket> GetTickets();
    Ticket GetTicket(int ticketNumber);
    Ticket AddTicket(int accountId, string question);
    Ticket AddTicket(int accountId, string device, string problem);
    void ChangeTicket(Ticket ticket);
    void ChangeTicketStateToClosed(int ticketNumber);
    void RemoveTicket(int ticketNumber);

    IEnumerable<TicketResponse> GetTicketResponses(int ticketNumber);
    TicketResponse AddTicketResponse(int ticketNumber, string response, bool isClientResponse);
  }
}
