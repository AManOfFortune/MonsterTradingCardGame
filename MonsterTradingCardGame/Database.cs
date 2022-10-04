using Npgsql;

namespace MonsterTradingCardGame
{
    public sealed class Database
    {
        private static readonly Lazy<Database> Lazy = new(() => new Database());

        public static Database Instance { get { return Lazy.Value; } }

        private readonly NpgsqlConnection _connection;

        private Database()
        {
            var cs = "Host=localhost;Username=postgres;Password=trust;Database=monstertradingcardsgame";

            _connection = new NpgsqlConnection(cs);
            _connection.Open();

            var sql = "SELECT version()";

            var cmd = new NpgsqlCommand(sql, _connection);

            var version = cmd.ExecuteScalar().ToString();

            Console.WriteLine($"PostgreSQL version: {version}");
        }

        ~Database()
        {
            _connection.Close();
        }

        public string Query(string query)
        {
            try
            {
                var cmd = new NpgsqlCommand(query, _connection);

                cmd.ExecuteNonQuery();

                return "Success!";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
