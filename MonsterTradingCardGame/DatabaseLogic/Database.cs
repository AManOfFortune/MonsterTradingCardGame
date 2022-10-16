using System.Data;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace MonsterTradingCardGame
{
    internal sealed class Database
    {
        private static readonly Lazy<Database> Lazy = new(() => new Database());

        public static Database Instance { get { return Lazy.Value; } }

        private readonly NpgsqlConnection _connection;

        private Database()
        {
            var cs = "Host=localhost;Username=postgres;Password=trust;Database=monstertradingcardsgame";

            _connection = new NpgsqlConnection(cs);
            _connection.Open();

            Console.WriteLine("\n------------------------------------------------------");
            Console.WriteLine("Database Connection established!");
            Console.WriteLine("------------------------------------------------------\n");
        }

        ~Database()
        {
            _connection.Close();
        }

        public DatabaseResponse Query(string query, JObject? parameters = null)
        {
            try
            {
                var cmd = new NpgsqlCommand(query, _connection);

                if (parameters != null)
                {
                    foreach (var parameter in parameters.Properties())
                    {
                        var sqlParam = new NpgsqlParameter("@" + parameter.Name, parameter.Value.ToString());
                        
                        cmd.Parameters.Add(sqlParam);
                    }
                }
                // Makes sure all our results are strings, no matter what data type they are in the database
                cmd.AllResultTypesAreUnknown = true;
                // Executes the query
                var reader = cmd.ExecuteReader();

                var response = new DatabaseResponse(reader);
                
                // Closes the reader to signal the database that the query is finished
                reader.Close();

                return response;
            }
            catch(Exception ex)
            {
                return new DatabaseResponse(ex.Message);
            }
        }
    }
}
