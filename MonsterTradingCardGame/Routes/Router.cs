using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.Routes
{
    internal class Router
    {
        // {
        //   GET: {
        //          "/users": func,
        //          "/session": func
        //        },
        //   POST: { ... }
        // }   
        private readonly Dictionary<string, Dictionary<string, Func<JObject, string>>> _routes = new();

        public void Add(string method, string location, Func<JObject, string> route)
        {
            _routes[method] = new Dictionary<string, Func<JObject, string>>() { { location, route } };
        }

        public string Route(JObject parsedRequest)
        {
            var method = parsedRequest["Method"].ToString();
            var location = parsedRequest["Location"].ToString();

            // Check if requested route exists
            if (!_routes.ContainsKey(method)) // Allowed/Supported Method
                return HttpServer.CreateResponse(405);
            if (!_routes[method].ContainsKey(location)) // Allowed/Supported locations
                return HttpServer.CreateResponse(404);

            // If user does not want to do an action yet (OPTIONS request was sent), return a positive response
            if (!parsedRequest["DoAction"].ToObject<bool>())
                return HttpServer.CreateResponse(200);

            // If user wants to do an action, call the associated function and return its response
            return _routes[parsedRequest["Method"].ToString()][parsedRequest["Location"].ToString()](parsedRequest["Content"].ToObject<JObject>());
        }
    }
}
