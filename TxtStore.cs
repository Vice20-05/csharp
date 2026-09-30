using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

// Stores users in a plain text file: users.txt
// Each line looks like: id|username|hash|timestamp
class TxtStore
{
    const string FilePath = "users.txt";
    const string Header = "id|username|passwordhash|createdat";
    const char Separator = '|';
    const int Columns = 4;

    // Make sure the file exists so we can read it safely.
    public static void Init()
    {
        if (!File.Exists(FilePath))
        {
            File.WriteAllText(FilePath, Header + Environment.NewLine);
        }
    }

    // --- THIS is the "one thread" part ---
    // Reads every line of the file on ONE background thread, waits for it,
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

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Trim().Length == 0)
                continue;

            // Skip anything that is not a full row, so a missing or repeated
            // header can never silently drop a real user.
            string[] parts = lines[i].Split(Separator);
            if (parts.Length != Columns)
                continue;
            if (parts[0].Trim() == "id")   // the header row
                continue;

            rows.Add(parts);
        }

        return rows;
    }

    // Stands in for SQLite's AUTOINCREMENT: the next id is the highest one
    // already used, plus one. Starts at 1 when the file has no users yet.
    static int NextId(List<string[]> rows)
    {
        int highest = 0;
        for (int i = 0; i < rows.Count; i++)
        {
            int id;
            if (int.TryParse(rows[i][0], out id) && id > highest)
                highest = id;
        }
        return highest + 1;
    }

    // Register works already: hashes the password and stores a UTC timestamp.
    public static void Register(string username, string password)
    {
        List<string[]> rows = ReadAll();

        string hash = PasswordHasher.Encode(password, 3);
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Unix seconds (UTC)
        string id = NextId(rows).ToString();

        rows.Add(new string[] { id, username, hash, timestamp.ToString() });
        WriteAll(rows);

        Console.WriteLine("User registered: " + username + " (hash: " + hash + ")");
    }

    // Checks username + password.
    public static bool Login(string username, string password)
    {
        List<string[]> rows = ReadAll();

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i][1] == username)
            {
                return PasswordHasher.Verify(password, rows[i][2]);
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
            if (rows[i][1] == username)
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
            if (rows[i][1] == username)
            {
                rows[i][2] = PasswordHasher.Encode(newPassword, 3);
                WriteAll(rows);
                Console.WriteLine("Password updated for: " + username);
                return;
            }
        }

        Console.WriteLine("User not found: " + username);
    }

    // Helper: writes the header + every row back to the file.
    public static void WriteAll(List<string[]> rows)
    {
        List<string> lines = new List<string>();
        lines.Add(Header);
        for (int i = 0; i < rows.Count; i++)
        {
            lines.Add(rows[i][0] + Separator + rows[i][1] + Separator
                    + rows[i][2] + Separator + rows[i][3]);
        }
        File.WriteAllLines(FilePath, lines.ToArray());
    }
}
