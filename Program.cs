using System;

class Program
{
    // The whole program runs inside try / catch / finally.
    static void Main(string[] args)
    {
        try
        {
            TxtStore.Init();   // creates users.txt (with its header) if missing

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

    // The menu loop: keeps repeating until the user picks 5 (Exit).
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
            Console.WriteLine("6. Exit");
            Console.Write("Choice: ");

            string choice = Console.ReadLine();
            switch (choice)   // every case calls one TxtStore method
            {
                case "1":     // Register: hash + save the new user
                    Console.Write("Username: ");
                    string u = Console.ReadLine();
                    Console.Write("Password: ");
                    string p = Console.ReadLine();
                    TxtStore.Register(u, p);
                    break;

                case "2":     // Login: true only if the password matches
                    Console.Write("Username: ");
                    string name = Console.ReadLine();
                    Console.Write("Password: ");
                    string pass = Console.ReadLine();

                    bool ok = TxtStore.Login(name, pass);
                    if (ok)
                        Console.WriteLine("Welcome, " + name + "!");
                    else
                        Console.WriteLine("Invalid credentials");
                    break;

                case "3":     // Update: GDPR right to rectification
                    Console.Write("Username: ");
                    string uname = Console.ReadLine();
                    Console.Write("New password: ");
                    string newpass = Console.ReadLine();
                    TxtStore.UpdateUser(uname, newpass);
                    break;

                case "4":     // Delete: GDPR right to erasure
                    Console.Write("Username: ");
                    string delUser = Console.ReadLine();
                    TxtStore.DeleteUser(delUser);
                    break;

                case "5":     // Show users: list everyone in the file
                    TxtStore.ShowUsers();
                    break;

                case "6":     // Exit: stop the loop
                    running = false;
                    break;

                default:       // anything else
                    Console.WriteLine("Unknown choice.");
                    break;
            }
        }
    }
}