using System;
using System.Collections.Generic;

namespace PayrollSystem
{
    // Interface for Payroll Calculation
    interface IPayroll
    {
        double CalculateNetSalary();
        void DisplayPaySlip();
    }

    // Base Abstract Class: Employee
    abstract class Employee : IPayroll
    {
        public int EmpId { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }

        public Employee(int id, string name, string dept)
        {
            EmpId = id;
            Name = name ?? "";
            Department = dept ?? "";
        }

        public abstract double CalculateNetSalary();

        public virtual void DisplayPaySlip()
        {
            Console.WriteLine($"\n========================================");
            Console.WriteLine($"           EMPLOYEE PAYSLIP             ");
            Console.WriteLine($"========================================");
            Console.WriteLine($"Employee ID   : {EmpId}");
            Console.WriteLine($"Employee Name : {Name}");
            Console.WriteLine($"Department    : {Department}");
        }
    }

    // Derived Class 1: Full-Time Employee (Inheritance & Polymorphism)
    class FullTimeEmployee : Employee
    {
        public double BasicSalary { get; set; }
        public double HRA => BasicSalary * 0.20; // 20% HRA
        public double DA => BasicSalary * 0.10;  // 10% DA
        public double PF => BasicSalary * 0.12;  // 12% PF Deduction

        public FullTimeEmployee(int id, string name, string dept, double basicSalary)
            : base(id, name, dept)
        {
            BasicSalary = basicSalary;
        }

        public override double CalculateNetSalary()
        {
            return BasicSalary + HRA + DA - PF;
        }

        public override void DisplayPaySlip()
        {
            base.DisplayPaySlip();
            Console.WriteLine($"Employment    : Full-Time Employee");
            Console.WriteLine($"Basic Salary  : ₹{BasicSalary:F2}");
            Console.WriteLine($"HRA (+20%)    : ₹{HRA:F2}");
            Console.WriteLine($"DA (+10%)     : ₹{DA:F2}");
            Console.WriteLine($"PF Deduct(-12%: ₹{PF:F2}");
            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"Net Salary    : ₹{CalculateNetSalary():F2}");
            Console.WriteLine($"========================================\n");
        }
    }

    // Derived Class 2: Part-Time Employee (Inheritance & Polymorphism)
    class PartTimeEmployee : Employee
    {
        public double HourlyRate { get; set; }
        public int HoursWorked { get; set; }

        public PartTimeEmployee(int id, string name, string dept, double hourlyRate, int hoursWorked)
            : base(id, name, dept)
        {
            HourlyRate = hourlyRate;
            HoursWorked = hoursWorked;
        }

        public override double CalculateNetSalary()
        {
            return HourlyRate * HoursWorked;
        }

        public override void DisplayPaySlip()
        {
            base.DisplayPaySlip();
            Console.WriteLine($"Employment    : Part-Time Employee");
            Console.WriteLine($"Hourly Rate   : ₹{HourlyRate:F2}");
            Console.WriteLine($"Hours Worked  : {HoursWorked} hrs");
            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"Net Salary    : ₹{CalculateNetSalary():F2}");
            Console.WriteLine($"========================================\n");
        }
    }

    // Derived Class 3: Contract Employee (Inheritance & Polymorphism)
    class ContractEmployee : Employee
    {
        public double ContractAmount { get; set; }
        public double TaxDeduction => ContractAmount * 0.05; // 5% Tax

        public ContractEmployee(int id, string name, string dept, double contractAmount)
            : base(id, name, dept)
        {
            ContractAmount = contractAmount;
        }

        public override double CalculateNetSalary()
        {
            return ContractAmount - TaxDeduction;
        }

        public override void DisplayPaySlip()
        {
            base.DisplayPaySlip();
            Console.WriteLine($"Employment    : Contract Employee");
            Console.WriteLine($"Contract Amt  : ₹{ContractAmount:F2}");
            Console.WriteLine($"Tax Deduct(-5%): ₹{TaxDeduction:F2}");
            Console.WriteLine($"----------------------------------------");
            Console.WriteLine($"Net Salary    : ₹{CalculateNetSalary():F2}");
            Console.WriteLine($"========================================\n");
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("===== EMPLOYEE PAYROLL MANAGEMENT SYSTEM =====\n");

            List<IPayroll> payrollList = new List<IPayroll>();

            while (true)
            {
                Console.WriteLine("1. Add Full-Time Employee");
                Console.WriteLine("2. Add Part-Time Employee");
                Console.WriteLine("3. Add Contract Employee");
                Console.WriteLine("4. Display All Pay Slips (Polymorphic Processing)");
                Console.WriteLine("5. Exit");
                Console.Write("Enter your choice (1-5): ");

                string choice = Console.ReadLine() ?? "";

                if (choice == "5") break;

                if (choice == "4")
                {
                    if (payrollList.Count == 0)
                    {
                        Console.WriteLine("\nNo employee records found.\n");
                        continue;
                    }

                    Console.WriteLine("\n========================================");
                    Console.WriteLine("     PROCESSING PAYROLL POLYMORPHICALLY   ");
                    Console.WriteLine("========================================");

                    foreach (IPayroll emp in payrollList)
                    {
                        // Polymorphic method invocation via Interface
                        emp.DisplayPaySlip();
                    }
                    continue;
                }

                Console.Write("Enter Employee ID: ");
                int.TryParse(Console.ReadLine(), out int id);

                Console.Write("Enter Employee Name: ");
                string name = Console.ReadLine() ?? "";

                Console.Write("Enter Department: ");
                string dept = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter Monthly Basic Salary: ");
                        double.TryParse(Console.ReadLine(), out double basic);
                        payrollList.Add(new FullTimeEmployee(id, name, dept, basic));
                        Console.WriteLine("Full-Time Employee added successfully!\n");
                        break;

                    case "2":
                        Console.Write("Enter Hourly Rate: ");
                        double.TryParse(Console.ReadLine(), out double rate);
                        Console.Write("Enter Hours Worked: ");
                        int.TryParse(Console.ReadLine(), out int hours);
                        payrollList.Add(new PartTimeEmployee(id, name, dept, rate, hours));
                        Console.WriteLine("Part-Time Employee added successfully!\n");
                        break;

                    case "3":
                        Console.Write("Enter Total Contract Amount: ");
                        double.TryParse(Console.ReadLine(), out double contract);
                        payrollList.Add(new ContractEmployee(id, name, dept, contract));
                        Console.WriteLine("Contract Employee added successfully!\n");
                        break;

                    default:
                        Console.WriteLine("Invalid option. Please try again.\n");
                        break;
                }
            }

            if (!Console.IsInputRedirected)
            {
                Console.ReadKey();
            }
        }
    }
}
