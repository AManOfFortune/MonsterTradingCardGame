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
        private readonly Dictionary<string, Dictionary<string, Func<JObject?, HttpResponse>>> _routes = new();

        public void Add(string method, string location, Func<JObject?, HttpResponse> route)
        {
            _routes[method] = new Dictionary<string, Func<JObject?, HttpResponse>> { { location, route } };
        }

        public HttpResponse Route(HttpRequest request)
        {
            // Check if requested route exists
            if (!_routes.ContainsKey(request.Method)) // Allowed/Supported Method
                return HttpResponse.MethodNotAllowed;
            if (!_routes[request.Method].ContainsKey(request.Location)) // Allowed/Supported locations
                return HttpResponse.NotFound;

            // If user does not want to do an action yet (OPTIONS request was sent), return a positive response
            if (!request.DoAction)
                return HttpResponse.Ok;

            // If user wants to do an action, call the associated function and return its response
            try
            {
                return _routes[request.Method][request.Location](request.Content);
            }
            catch (Exception ex)
            {
                var response = HttpResponse.InternalServerError;
                response.Body.Add("Error: ", ex.Message);

                return response;
            }
        }
    }
}
