using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class GetUserData : Route
    {

        public string? UserToGetName;

        public GetUserData()
        {
            AuthLevelRequired = User.UserRoles.Normal;
        }

        public override HttpResponse Call()
        {
            // Get the user_id associated with the authorization token
            int userId = UserManager.Instance.GetUserId(AuthToken);

            // Get the user_name associated with the authorization token
            string userName = UserManager.Instance.GetUser(AuthToken).Name;

            // If user wants to view another users data, return unauthorized
            if (UserToGetName != userName)
                return HttpResponse.Forbidden.WithStatusMessage("You are not allowed to view another user's data!");

            var user = GetAllUserData(userId);

            if (user != null)
            {
                var userJson = JObject.FromObject(user);

                var response = HttpResponse.Ok;

                response.Body.Add("user", userJson);

                return response;
            }
            else
                return HttpResponse.InternalServerError;
        }

        public override bool AddData(JObject data)
        {
            if (!data.ContainsKey("LocationParams"))
                return false;

            UserToGetName = data["LocationParams"]!.ToString();

            return !string.IsNullOrEmpty(UserToGetName);
        }

        // Marked as static because it gets used in the DoBattle class too
        public static User? GetAllUserData(int userId)
        {
            var request = new DatabaseRequest("SELECT * FROM users WHERE user_id = @user_id");

            request.Data.Add("user_id", userId);

            var data = request.PerformQuery();

            if (data.ErrorMessage == null)
            {
                var user = new User(
                    int.Parse(data["user_id"][0]!),
                    data["username"][0]!,
                    data["role"][0]!,
                    int.Parse(data["elo"][0]!)
                );

                user.Coins = int.Parse(data["coins"][0]!);
                user.Bio = data["bio"][0];
                user.Image = data["image"][0];

                return user;
            }
            else
                return null;
        }
    }
}
