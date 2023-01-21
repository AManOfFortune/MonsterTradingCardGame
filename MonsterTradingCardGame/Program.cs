using MonsterTradingCardGame.ServerLogic;

namespace MonsterTradingCardGame
{
    class Program
    {
        static void Main(string[] args)
        {
            // UI Options
            // (1) Battle | (2) Manage Deck | (3) View Card Collection | (4) Store | (5) Scoreboard | (6) Exit

            // Creates a server on port 10001
            HttpServer server = new(10001);

            // Register a signal handler to stop the server and clean up resources when the user presses the CTRL+C key combination
            Console.CancelKeyPress += (sender, e) =>
            {
                server.Stop();

                // Cancel the operation
                e.Cancel = true;
            };

            // Starts the server
            server.Start();
        }
    }
}