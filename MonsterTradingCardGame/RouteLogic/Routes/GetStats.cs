using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class GetStats : Route
    {
        public GetStats()
        {
            AuthLevelRequired = User.UserRoles.Normal;
        }

        public override HttpResponse Call()
        {
            // Get the user_id associated with the authorization token
            int userId = UserManager.Instance.GetUserId(AuthToken);

            var data = GetUserScoreData(userId);

            if (data.ErrorMessage == null)
            {
                var responseBody = new JObject
                {
                    { "wins", data["wins"][0] },
                    { "losses", data["losses"][0] },
                    { "elo", data["elo"][0] },
                };

                var response = HttpResponse.Ok;

                response.Body = responseBody;

                return response;
            }
            else
                return HttpResponse.InternalServerError;
        }

        private DatabaseResponse GetUserScoreData(int userId)
        {
            var request = new DatabaseRequest("SELECT wins, losses, elo FROM users WHERE user_id = @user_id");

            request.Data.Add("user_id", userId);

            return request.PerformQuery();
        }
    }
}
