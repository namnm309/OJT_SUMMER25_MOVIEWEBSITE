using Npgsql;
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        var connectionString = "Host=localhost;Port=5432;Database=OJT_SUMMER25_Group2_Movie;Username=postgres;Password=12345";
        var sqlScript = File.ReadAllText("add_layout_columns.sql");
        
        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            
            Console.WriteLine("Connected to database successfully!");
            
            using var command = new NpgsqlCommand(sqlScript, connection);
            var result = command.ExecuteNonQuery();
            
            Console.WriteLine($"SQL script executed successfully! Rows affected: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
} 