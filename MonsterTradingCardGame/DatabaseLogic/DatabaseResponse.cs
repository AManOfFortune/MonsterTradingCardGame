using Npgsql;

namespace MonsterTradingCardGame
{
    internal class DatabaseResponse
    {
        public int Rows;
        public string? ErrorMessage;

        private readonly Dictionary<string, List<string>> _values = new ();

        public DatabaseResponse(NpgsqlDataReader reader)
        {
            while (reader.Read())
            {
                // Increment row count
                Rows++;

                // Loop columns
                for (int columnNr = 0; columnNr < reader.FieldCount; columnNr++)
                {
                    var columnName = reader.GetName(columnNr);

                    // Check if column name exists, if not, add it
                    if (!_values.ContainsKey(columnName)) _values.Add(columnName, new List<string>());

                    // Add value to list at columnName
                    _values[columnName].Add(reader.GetString(columnNr));
                }
            }
        }

        public DatabaseResponse(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public List<string> this[string key] => _values[key];
    }
}
