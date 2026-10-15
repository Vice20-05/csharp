using System;
using System.Collections.Generic;
using System.Linq;

public class LINQ
{
    public static void ShowSorting(Employee e1,Employee e2)
    {
        //passing it as  a new list
        List<Employee> people=new List<Employee>{e1,e2};
        Console.WriteLine("\n Original order:\n");
        DisplayPeople(people);

        //LINQ first: OrderBy does NOT change 'people', it gives back
        //a new sorted sequence, so the original list stays as it was
        var sortLinq=people.OrderBy(person=>person.Name);
        Console.WriteLine("\n Linq Sort: \n");
        DisplayPeople(sortLinq);
        Console.WriteLine("\n List after Linq (unchanged):\n");
        DisplayPeople(people);

        //Regular Sort: Sort() changes the list itself (in place)
        //and returns nothing (void)
        //compare the two names, not a name with an Employee
        people.Sort((per1,per2)=>per1.Name.CompareTo(per2.Name));
        Console.WriteLine("\n Regular sorting (list itself changed):\n");
        DisplayPeople(people);

    }

    private static void DisplayPeople(IEnumerable<Employee> people)
    {
        foreach(Employee person in people)
        {
            // The ? is a ternary operator aka an if/else  in short 
            //Here is a better to visualize it , 
            /*
            if (person is Manager ){
                job="Manager";
            }
            else{
                job="Employee";
            }
            */
            
            string job=person is Manager ? "Manager":"Employee";
            Console.WriteLine(person.Name+"="+job);
        }
    }

}