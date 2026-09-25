using System;

class Manager : Employee
{
    public int ManagingPeople { get; set; }

    public Manager(string name, double salary, int id, int managingPeople)
        : base(name, salary, id)
    {
        ManagingPeople = managingPeople;
    }

    public void ManagerInfo()
    {
        Console.WriteLine();
        Console.WriteLine("Manager \t" + Name + "\t$" + Salary + "\tID:\t" + Id
            + "\tManaging people\t" + ManagingPeople);
    }
}