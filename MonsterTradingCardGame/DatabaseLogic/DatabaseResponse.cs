using Npgsql;

namespace MonsterTradingCardGame.DatabaseLogic
{
    internal class DatabaseResponse
    {
        public int Rows;
        public int Cols;

        public string? ErrorMessage;

        // {
        // Columnname: ["item1", "item2", ...]
        // Column2: ["item1", "item2", ...]
        // }
        private readonly Dictionary<string, List<string?>> _values = new ();

        public DatabaseResponse(NpgsqlDataReader reader)
        {
            Cols = reader.FieldCount;

            while (reader.Read())
            {
                // Increment row count
                Rows++;

                // Loop columns
                for (int columnNr = 0; columnNr < Cols; columnNr++)
                {
                    var columnName = reader.GetName(columnNr);

                    // Check if column name exists, if not, add it
                    if (!_values.ContainsKey(columnName)) _values.Add(columnName, new List<string?>());

                    // Add value to list at columnName
                    try
                    {
                        _values[columnName].Add(reader.GetString(columnNr));
                    }
                    // GetString throws an error if the value is null, so we add the null value manually here
                    catch (Exception ex)
                    {
                        _values[columnName].Add(null);
                    }
                }
            }
        }

        public DatabaseResponse(string errorMessage)
        {
            Console.WriteLine("\n------------------------------------------------------");
            Console.WriteLine("[DATABASE ERROR] " + errorMessage);
            Console.WriteLine("------------------------------------------------------\n");

            ErrorMessage = errorMessage;
        }

        public List<string?> this[string columnName] => _values[columnName];
        public List<string?> this[int columnIndex] => _values.ElementAt(columnIndex).Value;

    }
}
