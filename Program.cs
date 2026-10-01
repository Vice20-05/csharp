using System;

class Program
{
    // The one store the whole program uses. It is declared as the interface,
    // so the menu never depends on the TxtStore class itself.
    static IUserStore store = new TxtStore();

    // The whole program runs inside try / catch / finally.
    static void Main(string[] args)
    {
        try
        {
            Console.Clear();
            store.Init();   // creates users.txt (with its header) if missing

            // Your original lab code, kept as is:
            Employee e1 = new Employee("Anna Kenzie", 2000, 5341);
            Manager e2 = new Manager("Dylan Kob", 5000, 5214, 5);
            e1.ShowInfo();
            e2.ManagerInfo();

            Console.WriteLine();
            Console.WriteLine("--- User system ---");
            Menu();
        }
        catch (Exception ex)
        {
            // Runs if anything went wrong, so the app never dies silently.
            Console.WriteLine("Error: " + ex.Message);
        }
        finally
        {
            // Runs either way, even after an error.
            Console.WriteLine();
            Console.WriteLine("Program ended.");
        }
    }

    // The menu loop: keeps repeating until the user picks 8 (Exit).
    static void Menu()
    {
        bool running = true;   // becomes false on Exit, which ends the loop
        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("1. Register new user");
            Console.WriteLine("2. Login");
            Console.WriteLine("3. Update user (GDPR)");
            Console.WriteLine("4. Delete user (GDPR)");
            Console.WriteLine("5. Show users");
            Console.WriteLine("6. Make director");
            Console.WriteLine("7. Logout");
            Console.WriteLine("8. Exit");
            ShowStatus();
            Console.Write("Choice: ");

            string choice = Console.ReadLine();
            switch (choice)   // every case calls one store method
            {
                case "1":     // Register: hash + save the new user
                    Console.Clear();
                    Console.Write("Username: ");
                    string u = Console.ReadLine();
                    Console.Write("Password: ");
                    string p = Console.ReadLine();
                    store.Register(u, p);
                    break;

                case "2":     // Login: true only if the password matches
                    Console.Write("Username: ");
                    string name = Console.ReadLine();
                    Console.Write("Password: ");
                    string pass = Console.ReadLine();

                    bool ok = store.Login(name, pass);
                    if (!ok)
                    {
                        Console.WriteLine("Invalid credentials");
                        break;
                    }

                    // The director gets a special welcome.
                    if (store.IsDirector(name))
                        Console.WriteLine("Welcome, " + name + "! (director)");
                    else
                        Console.WriteLine("Welcome, " + name + "!");
                    break;

                case "3":     // Update: GDPR right to rectification
                    Console.Clear();
                    Console.Write("Username: ");
                    string uname = Console.ReadLine();
                    Console.Write("New password: ");
                    string newpass = Console.ReadLine();
                    store.UpdateUser(uname, newpass);
                    break;

                case "4":     // Delete: GDPR right to erasure
                    Console.Clear();
                    Console.Write("Username: ");
                    string delUser = Console.ReadLine();
                    store.DeleteUser(delUser);
                    break;

                case "5":     // Show users: your row, or everyone if director
                    Console.Clear();
                    store.ShowUsers();
                    break;

                case "6":     // Make director: only ever one
                    Console.Clear();
                    Console.Write("Username of the new director: ");
                    string newDir = Console.ReadLine();
                    store.MakeDirector(newDir);
                    break;

                case "7":     // Logout: forget who is logged in
                    Console.Clear();
                    store.Logout();
                    Console.WriteLine("Logged out.");
                    break;

                case "8":     // Exit: stop the loop
                    running = false;
                    break;

                default:       // anything else
                    Console.WriteLine("Unknown choice.");
                    break;
            }
        }
    }

    // One small line above the menu showing who is logged in.
    static void ShowStatus()
    {
        if (store.LoggedInUser == null)
        {
            Console.WriteLine("Not logged in.");
            return;
        }

        if (store.IsDirector(store.LoggedInUser))
            Console.WriteLine("Logged in as " + store.LoggedInUser + " (director)");
        else
            Console.WriteLine("Logged in as " + store.LoggedInUser);
    }
}