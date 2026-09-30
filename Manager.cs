using System;

// An employee who also manages a team. : base(...) replaces Java's super(...).
class Manager : Employee
{
    public int ManagingPeople { get; set; }

    // Calls the Employee constructor with the first three values.
    public Manager(string name, double salary, int id, int managingPeople)
        : base(name, salary, id)
    {
        ManagingPeople = managingPeople;
    }

    // Prints the manager on one line, with the extra field.
    public void ManagerInfo()
    {
        Console.WriteLine();
        Console.WriteLine("Manager \t" + Name + "\t$" + Salary + "\tID:\t" + Id
            + "\tManaging people\t" + ManagingPeople);
    }
}