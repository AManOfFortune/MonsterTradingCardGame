using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.ServerLogic;


namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class RegisterUser : Route
    {
        public string? Username = null;
        public string? Password = null;

        public override HttpResponse Call()
        {
            var success = CreateNewUser(Username!, Password!);

            return success.ErrorMessage == null ? HttpResponse.Created.WithStatusMessage("Success! Log in to get started!") : HttpResponse.BadRequest.WithStatusMessage("Error! Username already exists!");
        }

        private DatabaseResponse CreateNewUser(string username, string password)
        {
            string role = "player";

            var request = new DatabaseRequest("INSERT INTO users(username, password, role) VALUES(@username, @password, @role)");

            // "Hidden" way to make someone an admin is by adding "admin" into their username
            if (username.Contains("admin"))
            {
                Username = username;
                role = "admin";
            }

            request.Data.Add("username", username);
            request.Data.Add("password", password);
            request.Data.Add("role", role);
            
            return request.PerformQuery();
        }
    }
}
