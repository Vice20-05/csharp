using System.Collections.Generic;

// What every user store must be able to do. TxtStore is the only one we have,
// but the menu in Program.cs only talks to this interface, so the storage
// could be swapped for something else later (SQLite, a database, a web API)
// without changing the menu.
interface IUserStore
{
    // Creates the backing file if it is missing.
    void Init();

    // Adds a new user. Refuses a name that is already taken.
    void Register(string username, string password);

    // true when the username exists and the password matches its hash.
    bool Login(string username, string password);

    // GDPR right to rectification: changes a user's password hash.
    void UpdateUser(string username, string newPassword);

    // GDPR right to erasure: removes a user's row.
    void DeleteUser(string username);

    // Every user row in the file, one string[] per user:
    // { id, username, passwordhash, createdat, role }
    List<string[]> ReadAll();

    // Shows the usernames the current account is allowed to update or delete.
    List<string> GetManageableUsernames();

    // Prints the users. A director sees everyone plus the hashes,
    // a normal logged-in user only sees their own row.
    void ShowUsers();

    // true when this username is the director.
    bool IsDirector(string username);

    // Appoints a username as the one and only director.
    // Returns false if a director already exists.
    bool MakeDirector(string username);

    // Promotes an existing account to manager.
    bool MakeManager(string username);

    // true when this username is a manager.
    bool IsManager(string username);

    // true when a director has been appointed.
    bool HasDirector();

    // Who is logged in right now, or null when nobody is.
    string? LoggedInUser { get; }

    // Forgets the logged-in user, so the next action needs a login again.
    void Logout();
}