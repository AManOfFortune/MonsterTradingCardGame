using MonsterTradingCardGame.DatabaseLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.RouteLogic.Routes
{
    internal class EditUserData : Route
    {
        public string? Name;
        public string? Bio;
        public string? Image;

        public string? UserToEditName { get; set; }

        public EditUserData()
        {
            AuthLevelRequired = User.UserRoles.Normal;
        }

        public override HttpResponse Call()
        {
            // Get the user_id associated with the authorization token
            int userId = UserManager.Instance.GetUserId(AuthToken);

            // Get the user_name associated with the authorization token
            string userName = UserManager.Instance.GetUser(AuthToken).Name;

            // If user wants to edit another users data, return unauthorized
            if (UserToEditName != userName)
                return HttpResponse.Forbidden.WithStatusMessage("Error! You cannot edit another user's data.");
            
            return UpdateUserData(userId, Name!, Bio!, Image!).ErrorMessage == null ? 
                HttpResponse.Ok.WithStatusMessage("Successfully updated your user data.") : 
                HttpResponse.InternalServerError;
        }

        public override bool AddData(JObject data)
        {
            if (!data.ContainsKey("LocationParams"))
                return false;

            UserToEditName = data["LocationParams"]!.ToString();

            return !string.IsNullOrEmpty(UserToEditName) && base.AddData(data);
        }

        private DatabaseResponse UpdateUserData(int userId, string newName, string newBio, string newImage)
        {
            var request = new DatabaseRequest("UPDATE users SET username = @username, bio = @bio, image = @image WHERE user_id = @user_id");

            request.Data.Add("username", newName);
            request.Data.Add("bio", newBio);
            request.Data.Add("image", newImage);
            request.Data.Add("user_id", userId);

            return request.PerformQuery();
        }
    }
}
