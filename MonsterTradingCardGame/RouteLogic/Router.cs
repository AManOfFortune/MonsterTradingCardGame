using System.Diagnostics;
using MonsterTradingCardGame.RouteLogic.Routes;
using Newtonsoft.Json.Linq;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.StateLogic;
using MonsterTradingCardGame.DatabaseLogic.DataModels;

namespace MonsterTradingCardGame.RouteLogic
{
    public class Router
    {
        // {
        //   GET: {
        //          "/users": IRoute Implementation,
        //          "/session": IRoute Implementation
        //        },
        //   POST: { ... }
        // }   
        private readonly Dictionary<string, Dictionary<string, Route>> _routes = new();

        // Add routes
        public Router()
        {
            // Register new users
            AddRoute("POST", "/users", new RegisterUser());

            // Login user
            AddRoute("POST", "/sessions", new LoginUser());

            // Create new package
            AddRoute("POST", "/packages", new AddPackage());

            // Aquire package
            // SPECIAL: This route is supposed to be "/transactions/packages", but /transactions itself is not used with other subroutes
            // Seeing as implementing this according to specification would break my parser, I've decided to ignore the "/packages" part.
            AddRoute("POST", "/transactions", new BuyPackage());

            // Show all cards of user
            AddRoute("GET", "/cards", new GetCards());

            // Show deck of user (all info)
            AddRoute("GET", "/deck", new GetDeck(GetDeck.Format.Normal));

            // Show deck of user (reduced info)
            AddRoute("GET", "/deck?format=plain", new GetDeck(GetDeck.Format.Plain));

            // Configure deck
            AddRoute("PUT", "/deck", new ConfigureDeck());

            // Get user data
            AddRoute("GET", "/users", new GetUserData());

            // Edit user data
            AddRoute("PUT", "/users", new EditUserData());

            // Get user stats
            AddRoute("GET", "/stats", new GetStats());

            // Get scoreboard
            AddRoute("GET", "/score", new GetScoreboard());

            // Do battle with another user
            // Depending if another eligible user is currently waiting to do battle, returns different responses
            AddRoute("POST", "/battles", new DoBattle());

            // Get all open trades
            AddRoute("GET", "/tradings", new GetTrades());

            // Create or Buy open trade
            AddRoute("POST", "/tradings", new CreateOrBuyTrade());

            // Delete open trade
            AddRoute("DELETE", "/tradings", new DeleteTrade());
        }

        public void AddRoute(string method, string location, Route route)
        {
            if(!_routes.ContainsKey(method))
                _routes[method] = new Dictionary<string, Route>();

            _routes[method][location] = route;
        }

        public HttpResponse Route(HttpRequest request)
        {
            // If the requested route contains a '.', the requested resource is a file
            if (request.Location.Contains('.'))
                return FileRoute(request);
            
            // Otherwise the requested route concerns game logic
            return ApiRoute(request);
        }

        private HttpResponse FileRoute(HttpRequest request)
        {
            // Path of the directory containing all to-be-served files
            // If this does not work, simply paste the absolute path to the "/client" folder as a string here
            var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory))) + @"\client";

            // If the requested resource is not requested via GET, the request is invalid
            if (request.Method != "GET") 
                return HttpResponse.BadRequest.WithStatusMessage("Use GET method to request files!");

            var fileSystemEntries = Directory.GetFileSystemEntries(path); // gets all file paths in the root directory
            
            for (int i = 0; i < fileSystemEntries.Length; i++)
            {
                // Convert absolute file path to route
                var route = fileSystemEntries[i].Substring(path.Length);
                route = route.Replace(@"\", "/");
                
                // Check if the requested resource matches the discovered file
                if (route == request.Location)
                {
                    string fileContent = File.ReadAllText(fileSystemEntries[i]); // Read the file content
                    string fileExtension = Path.GetExtension(fileSystemEntries[i]); // Read file extension
                    
                    switch(fileExtension)
                    {
                        case ".html":
                            return HttpResponse.OkWithHtml.WithFileContent(fileContent);
                        case ".css":
                            return HttpResponse.OkWithCss.WithFileContent(fileContent);
                        case ".js":
                            return HttpResponse.OkWithJs.WithFileContent(fileContent);
                        default:
                            return HttpResponse.MediaTypeNotSupported.WithStatusMessage("The media with type " + fileExtension + " exists but is currently not supported.");
                    }
                }
            }

            // If no match was found, the requested resource does not exist
            return HttpResponse.NotFound.WithStatusMessage("The requested file was not found!");
        }

        private HttpResponse ApiRoute(HttpRequest request)
        {
            // Check if requested route exists
            if (!_routes.ContainsKey(request.Method)) // Allowed/Supported Method
                return HttpResponse.MethodNotAllowed;
            if (!_routes[request.Method].ContainsKey(request.Location)) // Allowed/Supported locations
                return HttpResponse.NotFound.WithStatusMessage("Route not found!");

            // If user does not want to do an action yet (OPTIONS request was sent), return a positive response
            if (!request.DoAction)
                return HttpResponse.Ok.WithStatusMessage("Proceed");

            // Add data to route
            // If function returns false, data was incomplete/not in the expected format
            if (!_routes[request.Method][request.Location].AddData(request.Content))
                return HttpResponse.BadRequest.WithStatusMessage("Error! Your provided data was incomplete/invalid.");

            // If route requires authentication (authLevel is not NONE), check if user has provided correct authentication
            // If all 3 checks pass, user is authorized and request can proceed
            if(_routes[request.Method][request.Location].AuthLevelRequired != User.UserRoles.None)
            {
                // 1. Check if user provided authentication token
                if (request.Authorization == null)
                    return HttpResponse.Unauthorized.WithStatusMessage("Error! No Authentication token was provided.");
                // 2. Check if user is logged in (== his token is valid)
                if(!UserManager.Instance.IsUserLoggedIn(request.Authorization))
                    return HttpResponse.Unauthorized.WithStatusMessage("Error! Authentication token invalid. Make sure you are logged in.");
                // 3. Check if user has the authentication level required
                if (!UserManager.Instance.DoesUserHavePermission(request.Authorization, _routes[request.Method][request.Location].AuthLevelRequired))
                    return HttpResponse.Forbidden.WithStatusMessage("Error! You do not have the required role to perform this action.");

                _routes[request.Method][request.Location].AuthToken = request.Authorization;
            }

            HttpResponse response;

            // If data was successfully added, call Route function
            try
            {
                response = _routes[request.Method][request.Location].Call();
            }
            // Fallback if anything unexpected fails
            catch (Exception ex)
            {
                Console.WriteLine("\n------------------------------------------------------");
                Console.WriteLine("[ERROR] " + ex.Message);
                Console.WriteLine("------------------------------------------------------\n");

                response = HttpResponse.InternalServerError;
            }
            
            return response;
        }
    }
}
