// Standalone script to create database
// Compile with: csc CreateDatabase.cs /r:Npgsql.dll /r:System.Data.dll
// Or use: dotnet script CreateDatabase.cs

using System;
using Npgsql;

class Program
{
    static void Main()
    {
        var adminConnStr = "Host=192.168.4.25;Database=postgres;Username=ai;Password=ai8989@";
        var targetDb = "teaching_assistant";
        
        Console.WriteLine($"Creating database '{targetDb}' on Anubis (192.168.4.25)...");
        
        using var conn = new NpgsqlConnection(adminConnStr);
        conn.Open();
        
        // Check if database exists
        var checkCmd = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{targetDb}';", conn);
        var exists = checkCmd.ExecuteScalar();
        
        if (exists == null)
        {
            // Create database
            var createCmd = new NpgsqlCommand($"CREATE DATABASE {targetDb};", conn);
            createCmd.ExecuteNonQuery();
            Console.WriteLine("✓ Database created successfully!");
        }
        else
        {
            Console.WriteLine("✓ Database already exists.");
        }
        
        conn.Close();
        
        // Now connect to the new database and create extensions
        var dbConnStr = $"Host=192.168.4.25;Database={targetDb};Username=ai;Password=ai8989@";
        using var dbConn = new NpgsqlConnection(dbConnStr);
        dbConn.Open();
        
        Console.WriteLine("Creating PostgreSQL extensions...");
        var extensions = new[] { "uuid-ossp", "pgcrypto", "vector", "pg_trgm" };
        foreach (var ext in extensions)
        {
            try
            {
                var extCmd = new NpgsqlCommand($"CREATE EXTENSION IF NOT EXISTS \"{ext}\";", dbConn);
                extCmd.ExecuteNonQuery();
                Console.WriteLine($"  ✓ Extension '{ext}' created/enabled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Failed to create extension '{ext}': {ex.Message}");
            }
        }
        
        Console.WriteLine("\n✓ Database setup complete!");
        Console.WriteLine("\nNext steps:");
        Console.WriteLine("1. Fix circular dependency in DotNetAgents projects");
        Console.WriteLine("2. Run: dotnet ef database update --project TeachingAssistant.Data --startup-project TeachingAssistant.API");
    }
}
