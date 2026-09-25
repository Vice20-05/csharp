using System;

class Employee
{
    public string Name { get; set; }
    public double Salary { get; set; }
    public int Id { get; set; }

    public Employee(string name, double salary, int id)
    {
        Name = name;
        Salary = salary;
        Id = id;
    }

    public void ShowInfo()
    {
        Console.WriteLine();
        Console.WriteLine(Name + "\t$" + Salary + "\tID:\t" + Id);
    }
}