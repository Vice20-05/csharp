using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

// Stores users in a simple CSV file: users.csv
// Each line looks like: username,hash,timestamp
class CsvStore
{
    const string FilePath = "users.csv";

    // Make sure the file exists so we can read it safely.
    public static void Init()
    {
        if (!File.Exists(FilePath))
        {
            File.WriteAllText(FilePath, "username,passwordhash,createdat" + Environment.NewLine);
        }
    }

    // --- THIS is the "one thread" part ---
    // Reads every line of the CSV on ONE background thread, waits for it,
    // then returns the rows as a list.
    public static List<string[]> ReadAll()
    {
        List<string[]> rows = new List<string[]>();
        string[] lines = null;

        Thread reader = new Thread(() =>
        {
            lines = File.ReadAllLines(FilePath);
        });

        reader.Start();   // start the thread
        reader.Join();    // wait for it to finish (one thread does the reading)

        for (int i = 1; i < lines.Length; i++)   // skip the header row
        {
            if (lines[i].Trim().Length == 0)
                continue;

            string[] parts = lines[i].Split(',');
            if (parts.Length == 3)
                rows.Add(parts);
        }

        return rows;
    }

    // Register works already: hashes the password and stores a UTC timestamp.
    public static void Register(string username, string password)
    {
        string hash = PasswordHasher.Encode(password, 3);
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Unix seconds (UTC)

        File.AppendAllText(FilePath,
            username + "," + hash + "," + timestamp + Environment.NewLine);

        Console.WriteLine("User registered: " + username + " (hash: " + hash + ")");
    }

    // Checks username + password.
    public static bool Login(string username, string password)
    {
        List<string[]> rows = ReadAll();

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i][0] == username)
            {
                return PasswordHasher.Verify(password, rows[i][1]);
            }
        }

        return false; // username not found
    }

    // GDPR "right to erasure": removes the user's row.
    public static void DeleteUser(string username)
    {
        List<string[]> rows = ReadAll();

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i][0] == username)
            {
                rows.RemoveAt(i);
                WriteAll(rows);
                Console.WriteLine("User deleted: " + username);
                return;
            }
        }

        Console.WriteLine("User not found: " + username);
    }

    // GDPR "right to rectification": changes the user's password hash.
    public static void UpdateUser(string username, string newPassword)
    {
        List<string[]> rows = ReadAll();

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i][0] == username)
            {
                rows[i][1] = PasswordHasher.Encode(newPassword, 3);
                WriteAll(rows);
                Console.WriteLine("Password updated for: " + username);
                return;
            }
        }

        Console.WriteLine("User not found: " + username);
    }

    // Helper for your TODOs: writes the header + every row back to the file.
    public static void WriteAll(List<string[]> rows)
    {
        List<string> lines = new List<string>();
        lines.Add("username,passwordhash,createdat");
        for (int i = 0; i < rows.Count; i++)
        {
            lines.Add(rows[i][0] + "," + rows[i][1] + "," + rows[i][2]);
        }
        File.WriteAllLines(FilePath, lines.ToArray());
    }
}