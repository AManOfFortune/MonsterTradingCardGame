using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class GetScoreboard : Route
    {
        public GetScoreboard()
        {
            AuthLevelRequired = User.UserRoles.Normal;
        }

        public override HttpResponse Call()
        {
            var data = GetAllUsers();

            if (data.ErrorMessage == null)
            {
                var usersList = new JArray();

                for(int i = 0; i < data.Rows; i++)
                {
                    var user = new JObject
                    {
                        { "name", data["username"][i] },
                        { "wins", data["wins"][i] },
                        { "losses", data["losses"][i] },
                        { "elo", data["elo"][i] }
                    };

                    usersList.Add(user);
                }
                
                // Orders the users list in descending order by their elo
                usersList = new JArray(usersList.OrderByDescending(obj => (int)obj["elo"]!));

                var response = HttpResponse.Ok.WithStatusMessage("Success! Scoreboard shows a total of " + data.Rows + " players.");

                response.Body.Add("scoreboard", usersList);

                return response;
            }
            else
                return HttpResponse.InternalServerError;
        }

        private DatabaseResponse GetAllUsers()
        {
            var request = new DatabaseRequest("SELECT * FROM users");

            return request.PerformQuery();
        }
    }
}
