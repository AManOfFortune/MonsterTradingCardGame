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
            // Starts the server
            // Currently a blocking function (endless loop)
            server.Start();
        }
    }
}