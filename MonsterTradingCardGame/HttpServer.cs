using System.Net;
using System.Net.Sockets;
using System.Text;
using MonsterTradingCardGame.Routes;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame
{
    internal class HttpServer
    {
        private readonly TcpListener _listener;
        private readonly Router _router;

        public HttpServer(int port)
        {
            // Creates a new TCP Listener with the given port
            _listener = new TcpListener(IPAddress.Any, port);

            // Creates and fills Router with routes
            _router = new Router();
            _router.Add("POST", "/users", Users.Instance.RegisterUser); // Register new users
            _router.Add("GET", "/users", Users.Instance.LoginUser); // Edit user data
        }

        public void Start()
        {
            // Starts the TCP Listener
            _listener.Start();

            // Enters an infinite loop to continuously accept clients
            while (true)
            {
                Console.WriteLine("Waiting for a connection...\n");

                // Accepts a client if one connects
                var client = _listener.AcceptTcpClient();

                // Clients gets handled in HandleClient function
                Console.WriteLine("------------------------------------------------------\n");
                HandleClient(client);
                Console.WriteLine("------------------------------------------------------\n");
            }
        }

        private void HandleClient(TcpClient client)
        {
            // Reads incoming request
            var buffer = new byte[1024];
            var stream = client.GetStream();

            var length = stream.Read(buffer, 0, buffer.Length);
            var incomingMessage = Encoding.UTF8.GetString(buffer, 0, length);

            // Parses HTTP request
            var parsedIncomingMessage = ParseRequest(incomingMessage);

            // Logs parsed incoming request
            Console.WriteLine($"Incoming request:\n{parsedIncomingMessage}\n");

            // Routes the request
            var response = _router.Route(parsedIncomingMessage);

            // Sends back response returned from router
            stream.Write(Encoding.UTF8.GetBytes(response));
        }
        
        private JObject ParseRequest(string httpRequest)
        {
            var reader = new StringReader(httpRequest);

            var parsedRequest = new JObject
            {
                { "Method", "" }, //GET, POST, ...
                { "Location", "" }, // /users, /sessions, ...
                { "DoAction", true }, // False if OPTIONS request is sent
                { "Content", "" } // { username: altenhof, password: phillip, ... }
            };

            // Loops each line of the request and fills parsedRequest JObject
            for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                // First line of request contains Method and Location
                if (line.Contains("HTTP"))
                {
                    parsedRequest["Method"] = line.Split(" ")[0];
                    parsedRequest["Location"] = "/" + line.Split(" ")[1].Split("/")[1];
                    continue;
                }

                // All other lines are either Headers or Content, each header we are interested in we save in the parsedRequest JObject
                switch (line.Split(": ")[0])
                {
                    case "Access-Control-Request-Method":
                        parsedRequest["DoAction"] = false;
                        parsedRequest["Method"] = line.Split(": ")[1];
                        break;
                }
            }

            // Content is located after two newline characters
            var content = httpRequest.Split(Environment.NewLine + Environment.NewLine)[1];

            // If Content is "", the request either has no content, or the "content" is placed after the initial location
            // F. ex. "/users/altenhof" -> "altenhof" is the content
            if (content.Length == 0)
            {
                // Isolate location string
                var firstLine = httpRequest.Split(Environment.NewLine)[0];
                var arrayOfThingsAfterLocation = firstLine.Split(" ")[1].Split(parsedRequest["Location"] + "/");

                // If the array produced by splitting the location string by the Method + / has more elements than 1, add that as content
                // F. ex. "/users/altenhof", split by "/users/", results into "["/users/", "altenhof"]
                // F. ex. "/session", split by "/session/", results into "["/session"]
                if (arrayOfThingsAfterLocation.Length > 1)
                    parsedRequest["Content"] = new JObject { { "locationParams", arrayOfThingsAfterLocation[1] } };
                // Otherwise, request contains no body so add an empty body
                else
                    parsedRequest["Content"] = new JObject();
            }
            // If content is not "", its regular JSON sent by the client
            else
            {
                parsedRequest["Content"] = JObject.Parse(content);
            }
            
            return parsedRequest;
        }

        public static string CreateResponse(int statusCode, string body = "")
        {
            Console.WriteLine($"Outgoing response:\n[{statusCode}] {body}\n");

            return "HTTP/1.0 " + statusCode + Environment.NewLine
                 + "Content-Length: " + body.Length + Environment.NewLine
                 + "Content-Type: " + "text/plain" + Environment.NewLine
                 + "Access-Control-Allow-Origin: *" + Environment.NewLine
                 + "Access-Control-Allow-Methods: POST, GET, OPTIONS" + Environment.NewLine
                 + "Access-Control-Allow-Headers: X-PINGOTHER, Content-Type" + Environment.NewLine
                 + Environment.NewLine
                 + body
                 + Environment.NewLine + Environment.NewLine;
        }
    }
}
