using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

// Stores users in a plain text file: users.txt
//
// The file looks like this:
//   id|username|passwordhash|createdat   <- header
//   lastid|3                              <- highest id ever used
//   director|anna                         <- the one director (optional)
//   1|anna|vhfuhw456|1759248000           <- one user
//   2|bob|mtvgmt012|1759248100
class TxtStore : IUserStore
{
    const string FilePath = "users.txt";
    const string Header = "id|username|passwordhash|createdat";
    const string LastIdLabel = "lastid";
    const string DirectorLabel = "director";
    const char Separator = '|';
    const int Columns = 4;

    // Who is logged in right now, or null when nobody is. Used by the menu
    // to decide which actions are allowed.
    public string? LoggedInUser { get; private set; } //the ? mark it allows null value

    // Forgets the logged-in user. Menu option 7.
    public void Logout()
    {
        LoggedInUser = null;
    }

    // --- THE ONE THREAD PART ---
    // Reads every line of the file on ONE background thread and waits for it.
    // Every method that touches the file goes through here.
    static string[] ReadLines()
    {
        string[]? lines = null;

        Thread reader = new Thread(() =>
        {
            lines = File.ReadAllLines(FilePath);
        });

        reader.Start();   // start the thread
        reader.Join();    // wait for it to finish (one thread does the reading)

        // The ! says "trust me, the thread always filled this in" - true
        // because Join() above has finished by the time we get here.
        return lines!;
    }

    // Turns the lines into rows, one row per user.
    static List<string[]> ParseRows(string[] lines)
    {
        List<string[]> rows = new List<string[]>();

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Trim().Length == 0)   // ignore blank lines
                continue;

            // Skip anything that is not a full row, so a missing or repeated
            // header can never silently drop a real user. This also skips the
            // lastid and director lines, because they only have 2 fields.
            string[] parts = lines[i].Split(Separator);
            if (parts.Length != Columns)
                continue;
            if (parts[0].Trim() == "id")   // the header row
                continue;

            rows.Add(parts);
        }

        return rows;
    }

    // Reads the small "label|value" lines: lastid and director.
    static string? ReadLabel(string label)
    {
        string[] lines = ReadLines();

        for (int i = 0; i < lines.Length; i++)
        {
            if (!lines[i].StartsWith(label + Separator))
                continue;

            string[] parts = lines[i].Split(Separator);
            if (parts.Length == 2)
                return parts[1];
        }

        return null;   // the line is not in the file
    }

    // The highest id we ever handed out. Returns 0 if there is no lastid line.
    static int ReadLastId()
    {
        string? text = ReadLabel(LastIdLabel);

        int value;
        if (text != null && int.TryParse(text, out value))
            return value;

        return 0;
    }

    // Every user row in the file.
    public List<string[]> ReadAll()
    {
        return ParseRows(ReadLines());
    }

    // Creates the file with its header and an empty id counter.
    public void Init()
    {
        if (File.Exists(FilePath))
            return;

        File.WriteAllLines(FilePath, new string[] { Header, LastIdLabel + Separator + 0 });
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

    // Adds a new user. This is the method behind menu option 1.
    public void Register(string username, string password)
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

    // Checks username + password. Menu option 2.
    public bool Login(string username, string password)
    {
        if (username == null || password == null)
            return false;

        List<string[]> rows = ReadAll();

        // Field 1 is the username, field 2 is the hash. First match wins.
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i][1] == username)
            {
                if (!PasswordHasher.Verify(password, rows[i][2]))
                    return false;

                LoggedInUser = username;   // remember who is logged in
                return true;
            }
        }

        return false; // username not found
    }

    // A director sees every user. A normal user only sees their own row.
    public void ShowUsers()
    {
        if (LoggedInUser == null)
        {
            Console.WriteLine("Please log in first (option 2).");
            return;
        }

        bool director = IsDirector(LoggedInUser);
        List<string[]> rows = ReadAll();
        int shown = 0;   // how many rows we actually printed

        if (director)
        {
            // The director gets the hashes too, which nobody else sees.
            Console.WriteLine("Id\tUsername\tHash\tCreated");
            for (int i = 0; i < rows.Count; i++)
            {
                Console.WriteLine(rows[i][0] + "\t" + rows[i][1] + "\t" + rows[i][2]
                    + "\t" + FormatDate(rows[i][3]));
                shown++;
            }
        }
        else
        {
            Console.WriteLine("Id\tUsername\tCreated");
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i][1] != LoggedInUser)
                    continue;   // a normal user only sees their own row

                Console.WriteLine(rows[i][0] + "\t" + rows[i][1] + "\t"
                    + FormatDate(rows[i][3]));
                shown++;
            }
        }

        Console.WriteLine(shown + " row(s) shown out of " + rows.Count + " stored.");
    }

    // Turns a stored Unix timestamp into a readable date.
    static string FormatDate(string storedSeconds)
    {
        long seconds = 0;
        long.TryParse(storedSeconds, out seconds);   // stays 0 if not a number

        DateTime when = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
        return when.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";
    }

    // Who is allowed to touch an account?
    // A director may touch anybody, a normal user only their own account.
    bool CanManage(string username)
    {
        if (LoggedInUser == null)
        {
            Console.WriteLine("Please log in first (option 2).");
            return false;
        }

        if (IsDirector(LoggedInUser))
            return true;   // the director is the super user

        if (LoggedInUser == username)
            return true;   // your own account

        Console.WriteLine("You can only change your own account.");
        return false;
    }

    // GDPR right to rectification: changes the user's password hash.
    // Menu option 3. The id and the createdat stay the same.
    public void UpdateUser(string username, string newPassword)
    {
        if (!CanManage(username))
            return;

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

    // GDPR right to erasure: removes the user's row. Menu option 4.
    public void DeleteUser(string username)
    {
        if (!CanManage(username))
            return;

        // The director is the one account that can never be removed, otherwise
        // somebody else could appoint a new director.
        if (IsDirector(username))
        {
            Console.WriteLine("The director account cannot be deleted.");
            return;
        }

        List<string[]> rows = ReadAll();

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i][1] == username)
            {
                rows.RemoveAt(i);   // drop that row
                WriteAll(rows);     // then save what is left
                Console.WriteLine("User deleted: " + username);

                if (username == LoggedInUser)
                    LoggedInUser = null;   // you deleted yourself
                return;             // stop, so we delete only one row
            }
        }

        Console.WriteLine("User not found: " + username);
    }

    // true once a director has been appointed.
    public bool HasDirector()
    {
        return ReadLabel(DirectorLabel) != null;
    }

    // true when this username is the director.
    public bool IsDirector(string username)
    {
        if (username == null)
            return false;

        return username == ReadLabel(DirectorLabel);
    }

    // Appoints a director. Only ever one, so a second attempt is refused.
    // Menu option 6.
    public bool MakeDirector(string username)
    {
        if (!IsValidUsername(username))
        {
            Console.WriteLine("Invalid username: it cannot be empty and cannot contain '|'");
            return false;
        }

        // Rule 1: only one director, ever.
        if (HasDirector())
        {
            Console.WriteLine("A director already exists: " + ReadLabel(DirectorLabel));
            return false;
        }

        // Rule 2: the director has to be a real account, so they can log in.
        List<string[]> rows = ReadAll();
        if (!UsernameExists(rows, username))
        {
            Console.WriteLine("No such user: " + username + " (register it first)");
            return false;
        }

        WriteLabel(DirectorLabel, username);   // saves and adds the director line

        Console.WriteLine(username + " is now the director.");
        return true;
    }

    // Adds or replaces one of the small "label|value" lines at the top of the
    // file, keeping it above the user rows.
    void WriteLabel(string label, string value)
    {
        string[] existing = ReadLines();   // same one-thread read as everywhere else
        List<string> lines = new List<string>();

        foreach (string line in existing)
        {
            if (line.Trim().Length == 0)
                continue;
            if (line.StartsWith(label + Separator))
                continue;   // drop the old value, we write a new one

            lines.Add(line);
        }

        // The header is always the first line, so the new line goes on line 2.
        lines.Insert(1, label + Separator + value);

        File.WriteAllLines(FilePath, lines.ToArray());
    }

    // Writes the header + the id counter + the director + every row.
    public void WriteAll(List<string[]> rows)
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

        string? director = ReadLabel(DirectorLabel);
        if (director != null)
            lines.Add(DirectorLabel + Separator + director);

        // Back to the file, one user per line, fields joined by '|'.
        for (int i = 0; i < rows.Count; i++)
        {
            lines.Add(rows[i][0] + Separator + rows[i][1] + Separator
                    + rows[i][2] + Separator + rows[i][3]);
        }

        File.WriteAllLines(FilePath, lines.ToArray());
    }
}