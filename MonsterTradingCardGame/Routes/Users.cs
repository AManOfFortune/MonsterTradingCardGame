using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.Routes
{
    public sealed class Users
    {
        private static readonly Lazy<Users> Lazy =
            new (() => new Users());

        public static Users Instance { get { return Lazy.Value; } }

        public string LoginUser(JObject data)
        {
            return HttpServer.CreateResponse(200, data.ToString());
        }

        public string RegisterUser(JObject data)
        {
            var message = Database.Instance.Query($"INSERT INTO users (username, password) VALUES ('{data["username"]}', '{data["password"]}')");

            return HttpServer.CreateResponse(200, message);
        }
    }
}
