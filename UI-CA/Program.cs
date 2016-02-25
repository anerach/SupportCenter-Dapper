using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SC.BL;
using SC.BL.Domain;
using SC.UI.CA.ExtensionMethods;

namespace SC.UI.CA
{
  class Program
  {
    private static bool quit = false;
    private static readonly ITicketManager mgr = new TicketManager();
    private static readonly Service srv = new Service();

    static void Main(string[] args)
    {
      while (!quit)
        ShowMenu();
    }

    private static void ShowMenu()
    {
      Console.WriteLine("=================================");
      Console.WriteLine("=== HELPDESK - SUPPORT CENTER ===");
      Console.WriteLine("=================================");
      Console.WriteLine("1) Toon alle tickets");
      Console.WriteLine("2) Toon details van een ticket");
      Console.WriteLine("3) Toon de antwoorden van een ticket");
      Console.WriteLine("4) Maak een nieuw ticket");
      Console.WriteLine("5) Geef een antwoord op een ticket");
      Console.WriteLine("6) Markeer ticket als 'Closed'");
      Console.WriteLine("0) Afsluiten");

            DetectMenuAction();
      /* try
      {
        DetectMenuAction();
      }
      catch (Exception e)
      {
        Console.WriteLine();
        Console.WriteLine("Er heeft zich een onverwachte fout voorgedaan!");
        Console.WriteLine();
      }*/
    }
    
    private static void DetectMenuAction()
    {
      bool inValidAction = false;
      do
      {
        Console.Write("Keuze: ");
        string input = Console.ReadLine();
        int action;
        if (Int32.TryParse(input, out action))
        {
          switch (action)
          {
            case 1:
              PrintAllTickets(); break;
            case 2:
              ActionShowTicketDetails(); break;
            case 3:
              ActionShowTicketResponses(); break;
            case 4:
              ActionCreateTicket(); break;
            case 5:
              ActionAddResponseToTicket(); break;
            case 6:
              ActionCloseTicket(); break;
            case 0:
              quit = true;
              return;
            default:
              Console.WriteLine("Geen geldige keuze!");
              inValidAction = true;
              break;
          }
        }
      } while (inValidAction);
    }

    private static void ActionCloseTicket()
    {
      Console.Write("Ticketnummer: ");
      int input = Int32.Parse(Console.ReadLine());

      //mgr.ChangeTicketStateToClosed(input);
      // via WebAPI-service
      srv.ChangeTicketStateToClosed(input);
    }

    private static void PrintAllTickets()
    {
      foreach (var t in mgr.GetTickets())
        Console.WriteLine(t.GetInfo());
    }

    private static void ActionShowTicketDetails()
    {
      Console.Write("Ticketnummer: ");
      int input = Int32.Parse(Console.ReadLine());

      Ticket t = mgr.GetTicket(input);
      PrintTicketDetails(t);
    }

    private static void PrintTicketDetails(Ticket ticket)
    {
      Console.WriteLine("{0,-15}: {1}", "Ticket", ticket.TicketNumber);
      Console.WriteLine("{0,-15}: {1}", "Gebruiker", ticket.AccountId);
      Console.WriteLine("{0,-15}: {1}", "Datum", ticket.DateOpened.ToString("dd/MM/yyyy"));
      Console.WriteLine("{0,-15}: {1}", "Status", ticket.State);

      if (ticket is HardwareTicket)
        Console.WriteLine("{0,-15}: {1}", "Toestel", ((HardwareTicket)ticket).DeviceName);

      Console.WriteLine("{0,-15}: {1}", "Vraag/probleem", ticket.Text);
    }

    private static void ActionShowTicketResponses()
    {
      Console.Write("Ticketnummer: ");
      int input = Int32.Parse(Console.ReadLine());

      //IEnumerable<TicketResponse> responses = mgr.GetTicketResponses(input);
      // via Web API-service
      IEnumerable<TicketResponse> responses = srv.GetTicketResponses(input);
      if (responses != null) PrintTicketResponses(responses);
    }

    private static void PrintTicketResponses(IEnumerable<TicketResponse> responses)
    {
      foreach (var r in responses)
        Console.WriteLine(r.GetInfo());
    }

    private static void ActionCreateTicket()
    {
      int accountNumber = 0;
      string problem = "";
      string device = "";
      
      Console.Write("Is het een hardware probleem (j/n)? ");
      bool isHardwareProblem = (Console.ReadLine().ToLower() == "j");
      if (isHardwareProblem)
      {
        Console.Write("Naam van het toestel: ");
        device = Console.ReadLine();
      }

      Console.Write("Gebruikersnummer: ");
      accountNumber = Int32.Parse(Console.ReadLine());
      Console.Write("Probleem: ");
      problem = Console.ReadLine();

      if (!isHardwareProblem)
        mgr.AddTicket(accountNumber, problem);
      else
        mgr.AddTicket(accountNumber, device, problem);
    }

    private static void ActionAddResponseToTicket()
    {
      Console.Write("Ticketnummer: ");
      int ticketNumber = Int32.Parse(Console.ReadLine());
      Console.Write("Antwoord: ");
      string response = Console.ReadLine();

      //mgr.AddTicketResponse(ticketNumber, response, false);
      // via WebAPI-service
      srv.AddTicketResponse(ticketNumber, response, false);
    }
  }
}