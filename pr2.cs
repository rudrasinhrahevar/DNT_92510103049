using System;

interface IPayroll
{
    void CalculateSalary();
}

class Employee
{
    protected int empId;
    protected string name;

    public Employee()
    {
        Console.WriteLine("Employee Payroll System\n");
    }

    public void GetDetails()
    {
        Console.Write("Enter Employee ID: ");
        empId = Convert.ToInt32(Console.ReadLine());

        Console.Write("Enter Employee Name: ");
        name = Console.ReadLine();
    }

    public virtual void Display()
    {
        Console.WriteLine("\nEmployee ID: " + empId);
        Console.WriteLine("Employee Name: " + name);
    }
}

class FullTimeEmployee : Employee, IPayroll
{
    private double basicSalary;
    private double netSalary;

    public void GetSalary()
    {
        Console.Write("Enter Basic Salary: ");
        basicSalary = Convert.ToDouble(Console.ReadLine());
    }

    public void CalculateSalary()
    {
        netSalary = basicSalary + (basicSalary * 0.20);
    }

    public override void Display()
    {
        base.Display();
        Console.WriteLine("Employee Type: Full Time");
        Console.WriteLine("Net Salary: " + netSalary);
    }
}

class PartTimeEmployee : Employee, IPayroll
{
    private int hours;
    private double rate;
    private double salary;

    public void GetSalary()
    {
        Console.Write("Enter Working Hours: ");
        hours = Convert.ToInt32(Console.ReadLine());

        Console.Write("Enter Hourly Rate: ");
        rate = Convert.ToDouble(Console.ReadLine());
    }

    public void CalculateSalary()
    {
        salary = hours * rate;
    }

    public override void Display()
    {
        base.Display();
        Console.WriteLine("Employee Type: Part Time");
        Console.WriteLine("Salary: " + salary);
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("1. Full Time Employee");
        Console.WriteLine("2. Part Time Employee");

        Console.Write("Enter Choice: ");
        int choice = Convert.ToInt32(Console.ReadLine());

        Employee emp;

        if (choice == 1)
        {
            FullTimeEmployee ft = new FullTimeEmployee();
            ft.GetDetails();
            ft.GetSalary();
            ft.CalculateSalary();
            emp = ft;
        }
        else
        {
            PartTimeEmployee pt = new PartTimeEmployee();
            pt.GetDetails();
            pt.GetSalary();
            pt.CalculateSalary();
            emp = pt;
        }

        emp.Display();

        Console.ReadKey();
    }
}