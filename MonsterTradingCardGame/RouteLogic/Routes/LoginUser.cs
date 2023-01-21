using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;


namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class LoginUser : Route
    {
        public string? Username = null;
        public string? Password = null;

        public override HttpResponse Call()
        {
            var data = GetUser(Username!, Password!);

            HttpResponse response;

            // If data has rows, it means the user was found
            if (data.Rows > 0)
            {
                response = HttpResponse.Ok;

                var userToken = Username + "-mtcgToken";

                // Save usertoken and send it back, if it already exists (NewUserLogin => false, meaning user is already logged in), then return bad request
                if (UserManager.Instance.NewUserLogin(userToken, int.Parse(data["user_id"][0]!)))
                    response.Body.Add("authToken", userToken);
                else
                    response = HttpResponse.BadRequest.WithStatusMessage("Error! User is already logged in!");
            }
            // Otherwise the user was not found and we return unauthorized
            else
            {
                response = HttpResponse.Unauthorized.WithStatusMessage("Username or Password not correct.");
            }

            return response;
        }

        private DatabaseResponse GetUser(string username, string password)
        {
            var request = new DatabaseRequest("SELECT user_id FROM users WHERE username = @username AND password = @password");

            request.Data.Add("username", username);
            request.Data.Add("password", password);
            
            return request.PerformQuery();
        }
    }
}
