using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using MonsterTradingCardGame.RouteLogic;

namespace MonsterTradingCardGame.ServerLogic
{
    internal class HttpServer
    {
        private readonly TcpListener _listener;
        private readonly List<Task> _clientThreadList = new();
        private static readonly Mutex GeneralMutex = new();

        private bool _serverStop = false;

        public HttpServer(int port)
        {
            // Creates a new TCP Listener with the given port
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            // Starts the TCP Listener
            _listener.Start();

            // Enters an infinite loop to continuously accept clients
            while (!_serverStop)
            {
                Console.WriteLine("Waiting for a connection...\n");

                // Accepts a client if one connects
                try
                {
                    var client = _listener.AcceptTcpClient();

                    Console.WriteLine("Client connected!");

                    // Clients gets handled in a seperate thread with the HandleClient function
                    Console.WriteLine("------------------------------------------------------\n");
                    Task task = Task.Run(() => HandleClient(client));
                    _clientThreadList.Add(task);
                }
                catch (Exception ex)
                {
                    // Swallow exceptions, because there should only be an exception caught here when the server is closed
                }
            }
        }

        public void Stop()
        {
            Console.WriteLine("Server stop initialized");

            _serverStop = true;

            // Stops the listener
            _listener.Stop();

            // Waits for all client communication to finish
            Task.WaitAll(_clientThreadList.ToArray());

            Console.WriteLine("Server stopped");
        }

        private void HandleClient(TcpClient client)
        {
            // Creates Router
            // TODO: Change router initialization behaviour, to only initialize classes when they are actually needed
            var router = new Router();

            // Reads incoming request
            var buffer = new byte[2048];
            var stream = client.GetStream();

            var length = stream.Read(buffer, 0, buffer.Length);
            var incomingMessage = Encoding.UTF8.GetString(buffer, 0, length);

            HttpRequest incomingRequest;
            HttpResponse outgoingResponse;

            try
            {
                // Parses incoming request
                incomingRequest = new(incomingMessage);

                // Logs parsed incoming request
                Console.WriteLine($"Incoming request:\n{JObject.FromObject(incomingRequest)}\n");
                
                // Routes the request
                // TODO: Don't lock mutex for the entire duration of the logic but only when it's actually needed
                GeneralMutex.WaitOne();
                outgoingResponse = router.Route(incomingRequest);
                GeneralMutex.ReleaseMutex();
                
                // Sends back response as a string
                stream.Write(Encoding.UTF8.GetBytes(outgoingResponse));
                
                // Logs outgoing response
                Console.WriteLine($"Outgoing response:\n[{outgoingResponse.StatusCode}] {outgoingResponse.Body}");
            }
            // If anything not caught elsewhere fails, log error and send bad request response
            // Most common errors caught here are JSON parsing errors
            catch (Exception ex)
            {
                Console.WriteLine("\n------------------------------------------------------");
                Console.WriteLine("[ERROR] " + ex.Message);
                Console.WriteLine("------------------------------------------------------\n");

                outgoingResponse = HttpResponse.BadRequest;

                stream.Write(Encoding.UTF8.GetBytes(outgoingResponse));
            }
            
            // Closes the client connection
            client.Close();
            Console.WriteLine("\n------------------------------------------------------");
        }
    }
}
