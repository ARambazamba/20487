using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BlueYonder.Flights.GroupProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.Function {
    public static class BookFlightFunc {
        [FunctionName ("BookFlightFunc")]
        public static IActionResult Run ([HttpTrigger (AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, TraceWriter log) {

            log.Info ("C# HTTP trigger function processed a request to the flights booking service.");

            var flightId = req.Query["flightId"];

            var flightServiceUrl = $"http://blueyonder-flights-ap.azurewebsites.net/api/flights/bookFlight?flightId={flightId}";

            log.Info ($"Flights service url: {flightServiceUrl}");

            var travelers = new List<Traveler> {
                new Traveler { Email = "204837Dazure@gmail.com", FirstName = "Jonathan", LastName = "James", MobilePhone = "+61 0658748", Passport = "204837DCBA" },
                new Traveler { Email = "204837Dfunction@gmail.com", FirstName = "James", LastName = "Barkal", MobilePhone = "+61 0658355", Passport = "204837DCBABC" }
            };

            var travelersAsJson = JsonConvert.SerializeObject (travelers);

            using (var client = new HttpClient ()) {
                client.PostAsync (flightServiceUrl,
                    new StringContent (travelersAsJson,
                        Encoding.UTF8,
                        "application/json")).Wait ();
            }

            return (ActionResult) new OkObjectResult ($"Request to book flight was sent successfully");
        }
    }
}