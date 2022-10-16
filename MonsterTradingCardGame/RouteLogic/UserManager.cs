using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.Routes
{
    internal sealed class UserManager
    {
        private static readonly Lazy<UserManager> Lazy = new (() => new UserManager());

        public static UserManager Instance => Lazy.Value;

        public HttpResponse LoginUser(JObject data)
        {
            var success = QueryManager.Instance.UserExists(data["username"].ToString(), data["password"].ToString());

            HttpResponse response;

            if (success)
            {
                response = HttpResponse.Ok;

                response.Body.Add("authToken", Guid.NewGuid());
            }
            else
            {
                response = new HttpResponse(401);
            }

            return response;
        }

        public HttpResponse RegisterUser(JObject data)
        {
            var success = QueryManager.Instance.CreateNewUser(data["username"].ToString(), data["password"].ToString());

            HttpResponse response;

            if (success.ErrorMessage == null)
            {
                response = HttpResponse.Created;

                response.Body.Add("authToken", Guid.NewGuid());
            }
            else
            {
                response = HttpResponse.BadRequest;
            }

            return response;
        }
    }
}
