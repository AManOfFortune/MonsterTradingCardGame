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

            RegisterRoutes();
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

                Console.WriteLine("Client connected!");

                // Clients gets handled in HandleClient function
                Console.WriteLine("------------------------------------------------------\n");
                HandleClient(client);
                Console.WriteLine("------------------------------------------------------\n");
            }
        }

        private void RegisterRoutes()
        {
            _router.Add("POST", "/users", UserManager.Instance.RegisterUser); // Register new users
            _router.Add("POST", "/sessions", UserManager.Instance.LoginUser); // Login user
        }

        private void HandleClient(TcpClient client)
        {
            // Reads incoming request
            var buffer = new byte[1024];
            var stream = client.GetStream();

            var length = stream.Read(buffer, 0, buffer.Length);
            var incomingMessage = Encoding.UTF8.GetString(buffer, 0, length);

            // Parses HTTP request
            HttpRequest incomingRequest = new (incomingMessage);

            // Logs parsed incoming request
            Console.WriteLine($"Incoming request:\n{JObject.FromObject(incomingRequest)}\n");

            // Routes the request
            var response = _router.Route(incomingRequest);

            Console.WriteLine($"Outgoing response:\n[{response.StatusCode}] {response.Body}");

            // Sends back response returned from router
            stream.Write(Encoding.UTF8.GetBytes(response.ToString()));
        }
    }
}
