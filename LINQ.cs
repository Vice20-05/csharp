using System;
using System.Collections.Generic;
using System.Linq;

public class LINQ
{
    public static void ShowSorting(Employee e1,Employee e2)
    {
        //Regular Sort
        //passing it as  a new list t
        List<Employee> people=new List<Employee>{e1,e1};
        //Sorting 
        people.Sort((per1,per2)=>per1.Name.CompareTo(per2));
        Console.WriteLine("\n Regular sorting:\n");

        //LINQ
        var sortLinq=people.OrderBy(person=>person.Name);
        Console.WriteLine("\n Linq Sort: \n");
        //To display the output of Linq sorting 
        DisplayPeople(sortLinq);

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