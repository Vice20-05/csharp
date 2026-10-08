using System;
using System.Net.Mail;

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
            if (string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine("Input cannot be empty.");
            }
        } while (string.IsNullOrWhiteSpace(value));

        return value;
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
            Console.WriteLine("8. Sort using Linq manager, employee");

            Console.WriteLine("9. Exit");
            ShowStatus();

            string choice = ReadRequiredInput("Choice: ");
            switch (choice)   // every case calls one store method
            {
                case "1":     // Register: hash + save the new user
                    Console.Clear();
                    string u = ReadRequiredInput("Username: ");
                    string p = ReadRequiredInput("Password: ");
                    store.Register(u, p);
                    break;

                case "2":     // Login: true only if the password matches
                    string name = ReadRequiredInput("Username: ");
                    string pass = ReadRequiredInput("Password: ");

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
                    string uname = ReadRequiredInput("Username: ");
                    string newpass = ReadRequiredInput("New password: ");
                    store.UpdateUser(uname, newpass);
                    break;

                case "4":     // Delete: GDPR right to erasure
                    Console.Clear();
                    string delUser = ReadRequiredInput("Username: ");
                    store.DeleteUser(delUser);
                    break;

                case "5":     // Show users: your row, or everyone if director
                    Console.Clear();
                    store.ShowUsers();
                    break;

                case "6":     // Make director: only ever one
                    Console.Clear();
                    string newDir = ReadRequiredInput("Username of the new director: ");
                    store.MakeDirector(newDir);
                    break;

                case "7":     // Logout: forget who is logged in
                    Console.Clear();
                    store.Logout();
                    Console.WriteLine("Logged out.");
                    break;

                case "8":
                    Console.Clear();
                    Console.WriteLine("Employee Name:\t");
                    string empName = ReadRequiredInput("");

                    Console.WriteLine("\nManager name: ");
                    string managerName = ReadRequiredInput("");

                    Employee employee = new Employee(empName, 2000, 5341);
                    Employee manager = new Manager(managerName, 5000, 5214, 5);

                    LINQ.ShowSorting(employee, manager);
                    break;

                case "9":     // Exit: stop the loop
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