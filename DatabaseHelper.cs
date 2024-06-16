using System.Text.RegularExpressions;
using System.Data.SQLite;
using System.Data;

public static class DatabaseHelper
{
    private static string connectionString = string.Empty;
    private static string configFile = @"..\..\..\config.conf";
    private static string databaseFile = @"..\..\..\database\library.db";

    private static string tableName = "Invoice";

    public static void InitializeDatabase()
    {
        string configText = File.ReadAllText(configFile);
        string pattern = @"connections_string=([^;]*);";

        Regex regex = new Regex(pattern);
        Match match = regex.Match(configText);

        if (match.Success)
            connectionString = match.Groups[1].Value;
        else
            throw new Exception("Connection string not found.");

        if (!File.Exists(configFile))
            throw new FileNotFoundException("Config file not found.");

        if (!File.Exists(databaseFile))
            SQLiteConnection.CreateFile(databaseFile);

        using var connection = new SQLiteConnection(connectionString);

        connection.Open();

        string query = $"SELECT * FROM sqlite_master WHERE type='table' AND name='{tableName}'";

        string createTable = $@"CREATE TABLE IF NOT EXISTS {tableName}
                (InvoID INTEGER PRIMARY KEY AUTOINCREMENT,
                InvoNr NVARCHAR,
                InvoLastUpdateDT DATETIME,
                InvoRemark NVARCHAR)";

        using var command = new SQLiteCommand(query, connection);

        if (command.ExecuteScalar() == null)
            Console.WriteLine($"New {tableName} table created,");

        command.CommandText = createTable;
        command.ExecuteNonQuery();

        connection.Close();

        Console.WriteLine("Database initialized successfully.");
    }

    public static void LoadDataAndUpdate()
    {
        string query = $"SELECT * FROM {tableName}";

        using var adapter = new SQLiteDataAdapter(query, connectionString);

        using DataSet dataSet = new();

        adapter.Fill(dataSet, tableName);

        if (dataSet.Tables[tableName] == null)
            throw new Exception($"\"{tableName}\" table does not exist.");

        using var commandBuilder = new SQLiteCommandBuilder(adapter);

        using DataTable table = dataSet.Tables[tableName];
        if (dataSet.Tables[tableName].Rows.Count == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                DataRow row = table.NewRow();
                row["InvoNr"] = "NR" + i;
                row["InvoLastUpdateDT"] = DateTime.Now;
                table.Rows.Add(row);
            }
            Console.WriteLine("New rows created.");
        }
        else
        {
            foreach (DataRow row in dataSet.Tables[tableName].Rows)
            {
                row["InvoLastUpdateDT"] = DateTime.Now;
            }
            Console.WriteLine("InvoLastUpdateDT updated.");
        }

        adapter.Update(dataSet, tableName);

        Console.WriteLine("Database updated successfully.");
    }
}
