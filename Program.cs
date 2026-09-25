using System;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            CsvStore.Init();

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
            Console.WriteLine("Error: " + ex.Message);
        }
        finally
        {
            Console.WriteLine();
            Console.WriteLine("Program ended.");
        }
    }

    static void Menu()
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("1. Register new user");
            Console.WriteLine("2. Login");
            Console.WriteLine("3. Update user (GDPR)");
            Console.WriteLine("4. Delete user (GDPR)");
            Console.WriteLine("5. Exit");
            Console.Write("Choice: ");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.Write("Username: ");
                    string u = Console.ReadLine();
                    Console.Write("Password: ");
                    string p = Console.ReadLine();
                    CsvStore.Register(u, p);
                    break;

                case "2":
                    Console.Write("Username: ");
                    string name = Console.ReadLine();
                    Console.Write("Password: ");
                    string pass = Console.ReadLine();

                    bool ok = CsvStore.Login(name, pass);
                    if (ok)
                        Console.WriteLine("Welcome, " + name + "!");
                    else
                        Console.WriteLine("Invalid credentials");
                    break;

                case "3":
                    Console.Write("Username: ");
                    string uname = Console.ReadLine();
                    Console.Write("New password: ");
                    string newpass = Console.ReadLine();
                    CsvStore.UpdateUser(uname, newpass);
                    break;

                case "4":
                    Console.Write("Username: ");
                    string delUser = Console.ReadLine();
                    CsvStore.DeleteUser(delUser);
                    break;

                case "5":
                    running = false;
                    break;

                default:
                    Console.WriteLine("Unknown choice.");
                    break;
            }
        }
    }
}