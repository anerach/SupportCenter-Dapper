using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using SC.BL;
using SC.BL.Domain;
using SC.UI.Web.MVC.Models;

namespace SC.UI.Web.MVC.Controllers.Api
{
    public class TicketController : ApiController
    {
        private ITicketManager mgr = new TicketManager();

        [Route("api/Ticket/{id}")]
        public IHttpActionResult GetSingle(int id)
        {
            Ticket ticket = mgr.GetTicket(id);
            if (ticket == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return Ok(ticket);
        }

        public IHttpActionResult Post([FromBody] Ticket ticket)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Ticket returnTicket = mgr.AddTicket(ticket.AccountId, ticket.Text);
            return CreatedAtRoute("DefaultApi", new { controller = "Ticket", TicketNumber = ticket.TicketNumber },
                returnTicket);
        }

        [HttpDelete]
        [Route("api/Ticket/{id}")]
        public IHttpActionResult Delete(int id)
        {
            Ticket ticketToDelete = mgr.GetTicket(id);
            if (ticketToDelete == null)
            {
                return NotFound();
            }

            mgr.RemoveTicket(id);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("api/Ticket/All")]
        public IHttpActionResult GetAll()
        {
            //IEnumerable<TicketResponse> responses = mgr.GetTicketResponses(ticketNumber);
            IEnumerable<Ticket> tickets = mgr.GetTickets();
            if (tickets == null || tickets.Count() == 0)
                return StatusCode(HttpStatusCode.NoContent);

            return Ok(tickets);
        }

        [HttpPut]
        [Route("api/Ticket/{id}/State/Closed")]
        public IHttpActionResult PutTicketStateToClosed(int id)
        {
            mgr.ChangeTicketStateToClosed(id);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
