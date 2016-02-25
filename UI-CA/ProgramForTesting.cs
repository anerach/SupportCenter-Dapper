using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

using SC.BL;
using SC.BL.Domain;
namespace SC.UI.CA
{
  class ProgramForTesting
  {
    static void Main(string[] args)
    {
      // (1) Ticket validation
      Ticket t1 = new Ticket()
      {
        TicketNumber = 1,
        AccountId = 1,
        Text = "",
        State = TicketState.Open,
        DateOpened = DateTime.Now
      };

      // (2) HardwareTicket validation
      Ticket t2 = new HardwareTicket()
      {
        TicketNumber = 2,
        AccountId = 1,
        DeviceName = "LPT-9876",
        Text = "text",
        State = TicketState.Open,
        DateOpened = DateTime.Now
      };

      // (3) TicketResponse with user-defined validation
      TicketResponse tr = new TicketResponse()
      {
        Id = 1,
        Text = "response",
        IsClientResponse = true,
        Date = new DateTime(2014, 1, 1),
        Ticket = new Ticket()
        {
          TicketNumber = 3,
          AccountId = 1,
          Text = "text",
          State = TicketState.Open,
          DateOpened = new DateTime(2015, 1, 1)
        }
      };

      // Validatie controle
      var errors = new List<ValidationResult>();
      Validator.TryValidateObject(tr, new ValidationContext(tr), errors, validateAllProperties: true);

      Console.ReadLine();
    }
  }
}
