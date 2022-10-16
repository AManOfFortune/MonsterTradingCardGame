using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame
{
    internal sealed class QueryManager
    {
        private static readonly Lazy<QueryManager> Lazy = new(() => new QueryManager());
        public static QueryManager Instance => Lazy.Value;

        public bool UserExists(string username, string password)
        {
            var data = new JObject
            {
                { "username", username },
                { "password", password }
            };
            
            var response = Database.Instance.Query("SELECT * FROM users WHERE username = @username AND password = @password", data);

            if (response.Rows == 0)
            {
                return false;
            }

            return true;
        }

        public DatabaseResponse CreateNewUser(string username, string password)
        {
            var data = new JObject
            {
                { "username", username },
                { "password", password }
            };

            var response = Database.Instance.Query("INSERT INTO users(username, password) VALUES(@username, @password)", data);

            return response;
        }
    }
}
