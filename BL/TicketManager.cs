using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SC.DAL;
using SC.BL.Domain;
using SC.DAL.Dapper;

namespace SC.BL
{
  public class TicketManager : ITicketManager
  {
    private readonly ITicketRepository repo;

    public TicketManager()
    {
      //repo = new TicketRepositoryHC();
      //repo = new SC.DAL.SqlClient.TicketRepository();
      //repo = new SC.DAL.EF.TicketRepository();
      repo = new DapperRepository();
      //repo = new DapperContribRepository();
    }

    public IEnumerable<Ticket> GetTickets()
    {
      return repo.ReadTickets();
    }

    public Ticket GetTicket(int ticketNumber)
    {
      return repo.ReadTicket(ticketNumber);
    }

    public Ticket AddTicket(int accountId, string question)
    {
      Ticket t = new Ticket()
      {
        AccountId = accountId,
        Text = question,
        DateOpened = DateTime.Now,
        State = TicketState.Open,
      };
      return this.AddTicket(t);
    }

    public Ticket AddTicket(int accountId, string device, string problem)
    {
      Ticket t = new HardwareTicket()
      {
        AccountId = accountId,
        Text = problem,
        DateOpened = DateTime.Now,
        State = TicketState.Open,
        DeviceName = device
      };
      return this.AddTicket(t);
    }

    private Ticket AddTicket(Ticket ticket)
    {
      this.Validate(ticket);
      return repo.CreateTicket(ticket);
    }

    public void ChangeTicket(Ticket ticket)
    {
      this.Validate(ticket);
      repo.UpdateTicket(ticket);
    }

    public void RemoveTicket(int ticketNumber)
    {
      repo.DeleteTicket(ticketNumber);
    }

    public IEnumerable<TicketResponse> GetTicketResponses(int ticketNumber)
    {
      return repo.ReadTicketResponsesOfTicket(ticketNumber);
    }

    public TicketResponse AddTicketResponse(int ticketNumber, string response, bool isClientResponse)
    {
      Ticket ticketToAddResponseTo = this.GetTicket(ticketNumber);
      if (ticketToAddResponseTo != null)
      {
        // Create response
        TicketResponse newTicketResponse = new TicketResponse();
        newTicketResponse.Date = DateTime.Now;
        newTicketResponse.Text = response;
        newTicketResponse.IsClientResponse = isClientResponse;
        newTicketResponse.Ticket = ticketToAddResponseTo;

        // Add response to ticket
        var responses = this.GetTicketResponses(ticketNumber);
        if (responses != null)
          ticketToAddResponseTo.Responses = responses.ToList();
        else
          ticketToAddResponseTo.Responses = new List<TicketResponse>();
        ticketToAddResponseTo.Responses.Add(newTicketResponse);

        // Change state of ticket
        if (isClientResponse)
          ticketToAddResponseTo.State = TicketState.ClientAnswer;
        else
          ticketToAddResponseTo.State = TicketState.Answered;


        // Validatie van ticketResponse en ticket afdwingen!!!
        this.Validate(newTicketResponse);
        this.Validate(ticketToAddResponseTo);

        // Bewaren naar db
        repo.CreateTicketResponse(newTicketResponse);
        repo.UpdateTicket(ticketToAddResponseTo);

        return newTicketResponse;
      }
      else
        throw new ArgumentException("Ticketnumber '" + ticketNumber + "' not found!");
    }

    public void ChangeTicketStateToClosed(int ticketNumber)
    {
      repo.UpdateTicketStateToClosed(ticketNumber);
    }

    private void Validate(Ticket ticket)
    {
      //Validator.ValidateObject(ticket, new ValidationContext(ticket), validateAllProperties: true);

      List<ValidationResult> errors = new List<ValidationResult>();
      bool valid = Validator.TryValidateObject(ticket, new ValidationContext(ticket), errors, validateAllProperties: true);

      if (!valid)
        throw new ValidationException(errors[0].ErrorMessage);
    }

    private void Validate(TicketResponse response)
    {
      //Validator.ValidateObject(response, new ValidationContext(response), validateAllProperties: true);

      List<ValidationResult> errors = new List<ValidationResult>();
      bool valid = Validator.TryValidateObject(response, new ValidationContext(response), errors, validateAllProperties: true);

      if (!valid)
        throw new ValidationException("TicketResponse not valid!");
    }
  }
}
