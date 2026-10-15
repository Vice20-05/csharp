using System;

// A base worker. Manager inherits from this and adds one field.
public class Employee
{
    public string Name { get; set; }
    public double Salary { get; set; }
    public int Id { get; set; }

    // Same constructor as in Java.
    public Employee(string name, double salary, int id)
    {
        Name = name;
        Salary = salary;
        Id = id;
    }

    // Prints the employee on one line.
    public void ShowInfo()
    {
        Console.WriteLine();
        Console.WriteLine(Name + "\t$" + Salary + "\tID:\t" + Id);
    }
}