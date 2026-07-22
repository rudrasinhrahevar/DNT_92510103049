using System;
using System.Text.RegularExpressions;

class Student
{
    private string name = "";
    private int age;
    private string phone = "";
    private string email = "";
    private string address = "";
    private double percentage;
    private string course = "";
    private bool scholarship;
    private string club = "";
    private string transport = "";

    public Student()
    {
        Console.WriteLine("===== STUDENT ADMISSION MANAGEMENT SYSTEM =====\n");
    }

    public void GetBasicDetails()
    {
        Console.Write("Enter Student Name : ");
        name = Console.ReadLine() ?? "";

        while (true)
        {
            Console.Write("Enter Age : ");
            string input = Console.ReadLine() ?? "";

            if (Regex.IsMatch(input, @"^\d{1,3}$"))
            {
                age = Convert.ToInt32(input);
                break;
            }

            Console.WriteLine("Invalid Age.");
        }

        while (true)
        {
            Console.Write("Enter Phone Number : ");
            phone = Console.ReadLine() ?? "";

            if (Regex.IsMatch(phone, @"^\d{10}$"))
                break;

            Console.WriteLine("Phone number must contain exactly 10 digits.");
        }

        while (true)
        {
            Console.Write("Enter Email : ");
            email = Console.ReadLine() ?? "";

            if (email.Contains("@") && email.Contains("."))
                break;

            Console.WriteLine("Invalid Email.");
        }

        Console.Write("Enter Address : ");
        address = Console.ReadLine() ?? "";
    }

    public void GetAcademicDetails()
    {
        while (true)
        {
            Console.Write("Enter 12th Percentage : ");

            if (double.TryParse(Console.ReadLine(), out percentage))
            {
                if (percentage >= 0 && percentage <= 100)
                    break;
            }

            Console.WriteLine("Invalid Percentage.");
        }

        if (percentage < 45)
        {
            Console.WriteLine("\nStudent is NOT Eligible for Admission.");
            Environment.Exit(0);
        }

        Console.WriteLine("\nStudent is Eligible for Admission.");
    }

    public void SelectCourse()
    {
        Console.WriteLine("\nAvailable Courses");
        Console.WriteLine("1. Computer Engineering");
        Console.WriteLine("2. Information Technology");
        Console.WriteLine("3. Mechanical Engineering");
        Console.WriteLine("4. Civil Engineering");
        Console.WriteLine("5. Electrical Engineering");
        Console.WriteLine("6. Artificial Intelligence");
        Console.WriteLine("7. Data Science");

        while (true)
        {
            Console.Write("Choose Course : ");

            int choice;

            if (int.TryParse(Console.ReadLine(), out choice))
            {
                switch (choice)
                {
                    case 1:
                        course = "Computer Engineering";
                        return;

                    case 2:
                        course = "Information Technology";
                        return;

                    case 3:
                        course = "Mechanical Engineering";
                        return;

                    case 4:
                        course = "Civil Engineering";
                        return;

                    case 5:
                        course = "Electrical Engineering";
                        return;

                    case 6:
                        course = "Artificial Intelligence";
                        return;

                    case 7:
                        course = "Data Science";
                        return;
                }
            }

            Console.WriteLine("Invalid Choice.");
        }
    }

    public void ScholarshipDetails()
    {
        if (percentage >= 90)
        {
            scholarship = true;
            Console.WriteLine("\nScholarship : 100%");
        }
        else if (percentage >= 80)
        {
            scholarship = true;
            Console.WriteLine("\nScholarship : 50%");
        }
        else if (percentage >= 70)
        {
            scholarship = true;
            Console.WriteLine("\nScholarship : 25%");
        }
        else
        {
            scholarship = false;
            Console.WriteLine("\nNo Scholarship");
        }
    }

    public void ClubSelection()
    {
        Console.WriteLine("\nSelect Club");
        Console.WriteLine("1. Coding Club");
        Console.WriteLine("2. Robotics Club");
        Console.WriteLine("3. Sports Club");
        Console.WriteLine("4. Music Club");
        Console.WriteLine("5. Dance Club");
        Console.WriteLine("6. None");

        while (true)
        {
            Console.Write("Choice : ");

            int choice;

            if (int.TryParse(Console.ReadLine(), out choice))
            {
                switch (choice)
                {
                    case 1:
                        club = "Coding Club";
                        return;

                    case 2:
                        club = "Robotics Club";
                        return;

                    case 3:
                        club = "Sports Club";
                        return;

                    case 4:
                        club = "Music Club";
                        return;

                    case 5:
                        club = "Dance Club";
                        return;

                    case 6:
                        club = "None";
                        return;
                }
            }

            Console.WriteLine("Invalid Choice.");
        }
    }

    public void TransportationDetails()
    {
        Console.Write("\nNeed Transportation? (Yes/No): ");
        string ans = (Console.ReadLine() ?? "").ToLower();

        if (ans == "yes")
        {
            transport = "College Bus";
        }
        else
        {
            transport = "Own Vehicle";
        }
    }

    public void DisplayAdmissionDetails()
    {
        Console.WriteLine("\n========== ADMISSION DETAILS ==========");

        Console.WriteLine("Student Name      : " + name);
        Console.WriteLine("Age               : " + age);
        Console.WriteLine("Phone             : " + phone);
        Console.WriteLine("Email             : " + email);
        Console.WriteLine("Address           : " + address);
        Console.WriteLine("12th Percentage   : " + percentage);
        Console.WriteLine("Selected Course   : " + course);
        Console.WriteLine("Scholarship       : " + (scholarship ? "Eligible" : "Not Eligible"));
        Console.WriteLine("Club              : " + club);
        Console.WriteLine("Transportation    : " + transport);

        Console.WriteLine("\nAdmission Successful.");
    }
}

class Program
{
    static void Main()
    {
        Student student = new Student();

        student.GetBasicDetails();

        student.GetAcademicDetails();

        student.SelectCourse();

        student.ScholarshipDetails();

        student.ClubSelection();

        student.TransportationDetails();

        student.DisplayAdmissionDetails();

        if (!Console.IsInputRedirected)
        {
            Console.ReadKey();
        }
    }
}