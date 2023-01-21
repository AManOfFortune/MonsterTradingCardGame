using Npgsql;

namespace MonsterTradingCardGame.DatabaseLogic
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

        public DatabaseResponse Query(DatabaseRequest request)
        {
            try
            {
                var cmd = new NpgsqlCommand(request.SqlQuery, _connection);

                // Add parameters to command
                foreach (var parameter in request.Data!)
                {
                    var sqlParam = new NpgsqlParameter("@" + parameter.Key, parameter.Value);

                    cmd.Parameters.Add(sqlParam);
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
