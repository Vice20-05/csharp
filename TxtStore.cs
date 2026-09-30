using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

// Stores users in a plain text file: users.txt
// Each user line looks like: id|username|hash|timestamp
// The second line is a counter: lastid|<n>
class TxtStore
{
    const string FilePath = "users.txt";
    const string Header = "id|username|passwordhash|createdat";
    const string LastIdLabel = "lastid";
    const char Separator = '|';
    const int Columns = 4;

    // Make sure the file exists so we can read it safely.
    public static void Init()
    {
        if (!File.Exists(FilePath))
        {
            File.WriteAllLines(FilePath, new string[] { Header, LastIdLabel + Separator + 0 });
        }
    }

    // --- THIS is the "one thread" part ---
    // Reads every line of the file on ONE background thread and waits for it.
    static string[] ReadLines()
    {
        string[] lines = null;

        Thread reader = new Thread(() =>
        {
            lines = File.ReadAllLines(FilePath);
        });

        reader.Start();   // start the thread
        reader.Join();    // wait for it to finish (one thread does the reading)

        return lines;
    }

    // Turns the file into a list of rows (one row per user).
    public static List<string[]> ReadAll()
    {
        List<string[]> rows = new List<string[]>();
        string[] lines = ReadLines();

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Trim().Length == 0)   // ignore blank lines
                continue;

            // Skip anything that is not a full row, so a missing or repeated
            // header can never silently drop a real user. This also skips the
            // lastid line, because it only has 2 fields.
            string[] parts = lines[i].Split(Separator);
            if (parts.Length != Columns)
                continue;
            if (parts[0].Trim() == "id")   // the header row
                continue;

            rows.Add(parts);
        }

        return rows;
    }

    // The highest id we ever handed out, remembered in the lastid line.
    // Returns 0 if the file has no such line yet.
    static int ReadLastId()
    {
        string[] lines = ReadLines();

        for (int i = 0; i < lines.Length; i++)
        {
            if (!lines[i].StartsWith(LastIdLabel + Separator))
                continue;

            string[] parts = lines[i].Split(Separator);
            int value;
            if (parts.Length == 2 && int.TryParse(parts[1], out value))
                return value;
        }

        return 0;
    }

    // Stands in for SQLite's AUTOINCREMENT: the next id is one above the
    // highest id ever used, so a deleted id is never handed out again.
    static int NextId(List<string[]> rows)
    {
        int last = ReadLastId();

        // A file from an older version has no lastid line, so also look at the
        // ids that are really in the file. That way ids never clash.
        for (int i = 0; i < rows.Count; i++)
        {
            int id;
            if (int.TryParse(rows[i][0], out id) && id > last)
                last = id;
        }

        return last + 1;
    }

    // A username sits inside one line of the file, so it cannot be empty and
    // it cannot contain the separator, which would split the row in two.
    static bool IsValidUsername(string username)
    {
        if (username == null || username.Trim().Length == 0)
            return false;
        if (username.Contains(Separator))
            return false;
        return true;
    }

    // SQLite had UNIQUE on Username. A text file has no such rule, so we
    // check the rows ourselves.
    static bool UsernameExists(List<string[]> rows, string username)
    {
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i][1] == username)
                return true;
        }
        return false;
    }

    // Register works already: hashes the password and stores a UTC timestamp.
    public static void Register(string username, string password)
    {
        if (!IsValidUsername(username))
        {
            Console.WriteLine("Invalid username: it cannot be empty and cannot contain '|'");
            return;
        }

        if (password == null || password.Length == 0)
        {
            Console.WriteLine("Invalid password: it cannot be empty");
            return;
        }

        List<string[]> rows = ReadAll();

        if (UsernameExists(rows, username))
        {
            Console.WriteLine("Username already taken: " + username);
            return;
        }

        string hash = PasswordHasher.Encode(password, 3);
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Unix seconds (UTC)
        string id = NextId(rows).ToString();

        rows.Add(new string[] { id, username, hash, timestamp.ToString() });
        WriteAll(rows);   // rewrites the whole file, counter included

        Console.WriteLine("User registered: " + username + " (hash: " + hash + ")");
    }

    // Checks username + password.
    public static bool Login(string username, string password)
    {
        if (username == null || password == null)
            return false;

        List<string[]> rows = ReadAll();

        // Field 1 is the username, field 2 is the hash. First match wins.
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
                rows.RemoveAt(i);   // drop that row
                WriteAll(rows);     // then save what is left
                Console.WriteLine("User deleted: " + username);
                return;             // stop, so we delete only one row
            }
        }

        Console.WriteLine("User not found: " + username);
    }

    // GDPR "right to rectification": changes the user's password hash.
    // The id and the createdat stay the same, so nothing else changes.
    public static void UpdateUser(string username, string newPassword)
    {
        if (newPassword == null || newPassword.Length == 0)
        {
            Console.WriteLine("Invalid password: it cannot be empty");
            return;
        }

        List<string[]> rows = ReadAll();

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i][1] == username)
            {
                rows[i][2] = PasswordHasher.Encode(newPassword, 3);   // only the hash
                WriteAll(rows);
                Console.WriteLine("Password updated for: " + username);
                return;
            }
        }

        Console.WriteLine("User not found: " + username);
    }

    // Helper: writes the header + the id counter + every row back to the file.
    public static void WriteAll(List<string[]> rows)
    {
        // The counter may never go backwards. If every user was deleted the
        // rows are empty, so without this the ids would start over at 1.
        int last = ReadLastId();
        for (int i = 0; i < rows.Count; i++)
        {
            int id;
            if (int.TryParse(rows[i][0], out id) && id > last)
                last = id;
        }

        List<string> lines = new List<string>();
        lines.Add(Header);
        lines.Add(LastIdLabel + Separator + last);

        // Back to the file, one user per line, fields joined by '|'.
        for (int i = 0; i < rows.Count; i++)
        {
            lines.Add(rows[i][0] + Separator + rows[i][1] + Separator
                    + rows[i][2] + Separator + rows[i][3]);
        }

        File.WriteAllLines(FilePath, lines.ToArray());
    }
}
