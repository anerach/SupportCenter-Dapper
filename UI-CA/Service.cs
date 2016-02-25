using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http; // add reference to 'System.Net.Http'
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json; // install NuGet-package 'Newtonsoft.Json'

using SC.BL.Domain;
namespace SC.UI.CA
{
  internal class Service
  {
    private const string baseUri = "http://localhost:51150/api/";
    //private const string baseUri = "http://localhost.fiddler:51150/api/"; // use this when using fiddler to capture traffic! 

    public IEnumerable<TicketResponse> GetTicketResponses(int ticketNumber)
    {
      IEnumerable<TicketResponse> responses = null;

      using (HttpClient http = new HttpClient())
      {
        string uri = baseUri + "TicketResponse?ticketNumber=" + ticketNumber;
        HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
        //Verwachte content-type van de response meegeven
        httpRequest.Headers.Add("Accept", "application/json");
        //Request versturen en wachten op de response
        HttpResponseMessage httpResponse = http.SendAsync(httpRequest).Result;
        if (httpResponse.IsSuccessStatusCode)
        {
          //Body van de response uitlezen als een string
          string responseContentAsString = httpResponse.Content.ReadAsStringAsync().Result;
          //Body-string (in json-format) deserializeren (omzetten) naar een verzameling van TicketResponse-objecten
          responses = JsonConvert.DeserializeObject<List<TicketResponse>>(responseContentAsString);
        }
        else
        {
          throw new Exception(httpResponse.StatusCode + " " + httpResponse.ReasonPhrase);
        }
      }

      return responses;
    }

    public TicketResponse AddTicketResponse(int ticketNumber, string response, bool isClientResponse)
    {
      TicketResponse tr = null;

      using (HttpClient http = new HttpClient())
      {
        string uri = baseUri + "TicketResponse";
        HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, uri);
        //Request data toevoegen aan body, via anonymous object dat je serialiseert naar json-formaat
        object data = new
        {
          TicketNumber = ticketNumber,
          ResponseText = response,
          IsClientResponse = isClientResponse
        };
        string dataAsJsonString = JsonConvert.SerializeObject(data);
        httpRequest.Content = new StringContent(dataAsJsonString, Encoding.UTF8, "application/json");
        //Verwachte content-type van de response meegeven
        httpRequest.Headers.Add("Accept", "application/json");
        //Request versturen en wachten op de response
        HttpResponseMessage httpResponse = http.SendAsync(httpRequest).Result;
        if (httpResponse.IsSuccessStatusCode)
        {
          //Body van de response uitlezen als een string
          string responseContentAsString = httpResponse.Content.ReadAsStringAsync().Result;
          //Body-string (in json-format) deserializeren (omzetten) naar een TicketResponse-object
          tr = JsonConvert.DeserializeObject<TicketResponse>(responseContentAsString);
        }
        else
        {
          throw new Exception(httpResponse.StatusCode + " " + httpResponse.ReasonPhrase);
        }
      }

      return tr;
    }

    public void ChangeTicketStateToClosed(int ticketNumber)
    {
      using (HttpClient http = new HttpClient())
      {
        string uri = baseUri + "Ticket/" + ticketNumber + "/State/Closed";
        HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Put, uri);
        //Request versturen en wachten op de response
        HttpResponseMessage httpResponse = http.SendAsync(httpRequest).Result;
        if (!httpResponse.IsSuccessStatusCode)
        {
          throw new Exception(httpResponse.StatusCode + " " + httpResponse.ReasonPhrase);
        }
      }
    }
  }
}
