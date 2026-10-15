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

    static string ReadRequiredInput(string prompt)
    {
        string? value;

        do
        {
            Console.Write(prompt);
            value = Console.ReadLine();

            // null means the input was closed (Ctrl+D / end of a piped file).
            // Asking again would loop forever, so stop the program instead.
            if (value == null)
                throw new EndOfStreamException("No more input.");

            if (string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine("Input cannot be empty.");
            }
        } while (string.IsNullOrWhiteSpace(value));

        return value;
    }

    // Usernames are trimmed, so "bob " and "bob" are the same account.
    static string ReadUsername(string prompt)
    {
        return ReadRequiredInput(prompt).Trim();
    }

    // The menu loop: keeps repeating until the user picks 9 (Exit).
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
            // There is only ever one director, so this is only offered
            // until the first one has been appointed.
            if (!store.HasDirector())
                Console.WriteLine("6. Make director");
            Console.WriteLine("7. Logout");
            Console.WriteLine("8. Sort using Linq manager, employee");
            Console.WriteLine("9. Exit");
            if (store.LoggedInUser != null && store.IsDirector(store.LoggedInUser))
                Console.WriteLine("10. Make manager");
            ShowStatus();

            string choice = ReadRequiredInput("Choice: ");
            switch (choice)   // every case calls one store method
            {
                case "1":     // Register: hash + save the new user
                    Console.Clear();
                    string u = ReadUsername("Username: ");
                    string p = ReadRequiredInput("Password: ");
                    store.Register(u, p);
                    break;

                case "2":     // Login: true only if the password matches
                    string name = ReadUsername("Username: ");
                    string pass = ReadRequiredInput("Password: ");

                    bool ok = store.Login(name, pass);
                    Console.Clear();
                    if (!ok)
                    {
                        Console.WriteLine("Invalid credentials");
                        break;
                    }

                    // The director gets a special welcome.
                    if (store.IsDirector(name))
                        Console.WriteLine("Welcome, " + name + "! (director)");
                    else if (store.IsManager(name))
                        Console.WriteLine("Welcome, " + name + "! (manager)");
                    else
                        Console.WriteLine("Welcome, " + name + "!");
                    break;

                case "3":     // Update: GDPR right to rectification
                    Console.Clear();
                    List<string> updateUsers = store.GetManageableUsernames();
                    if (updateUsers.Count == 0)
                        break;
                    string uname = ReadUsername("Username: ");
                    string newpass = ReadRequiredInput("New password: ");
                    store.UpdateUser(uname, newpass);
                    break;

                case "4":     // Delete: GDPR right to erasure
                    Console.Clear();
                    List<string> deleteUsers = store.GetManageableUsernames();
                    if (deleteUsers.Count == 0)
                        break;
                    string delUser = ReadUsername("Username: ");
                    store.DeleteUser(delUser);
                    break;

                case "5":     // Show users: your row, or everyone if director
                    Console.Clear();
                    store.ShowUsers();
                    break;

                case "6":     // Make director: only ever one
                    Console.Clear();
                    string newDir = ReadUsername("Username of the new director: ");
                    store.MakeDirector(newDir);
                    break;

                case "7":     // Logout: forget who is logged in
                    Console.Clear();
                    store.Logout();
                    Console.WriteLine("Logged out.");
                    break;

                case "8":
                    Console.Clear();
                    string empName = ReadRequiredInput("Employee name: ");
                    string managerName = ReadRequiredInput("Manager name: ");

                    Employee employee = new Employee(empName, 2000, 5341);
                    Employee manager = new Manager(managerName, 5000, 5214, 5);

                    LINQ.ShowSorting(employee, manager);
                    break;

                case "9":     // Exit: stop the loop
                    running = false;
                    break;

                case "10":
                    Console.Clear();
                    string newManager = ReadUsername("Username of the new manager: ");
                    store.MakeManager(newManager);
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
            return;

        if (store.IsDirector(store.LoggedInUser))
            Console.WriteLine("Logged in as " + store.LoggedInUser + " (director)");
        else if (store.IsManager(store.LoggedInUser))
            Console.WriteLine("Logged in as " + store.LoggedInUser + " (manager)");
        else
            Console.WriteLine("Logged in as " + store.LoggedInUser);
    }
}