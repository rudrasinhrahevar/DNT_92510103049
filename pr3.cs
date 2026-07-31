using System;

class Expense
{
    private string category;
    private double amount;

    public Expense()
    {
        Console.WriteLine("===== Expense Tracking Module =====\n");
    }

    public void GetExpense()
    {
        Console.Write("Enter Expense Category: ");
        category = Console.ReadLine();

        try
        {
            Console.Write("Enter Expense Amount: ");
            amount = Convert.ToDouble(Console.ReadLine());

            if (amount <= 0)
            {
                throw new Exception("Expense amount must be greater than 0.");
            }

            Console.WriteLine("\nExpense Added Successfully.");
        }
        catch (FormatException)
        {
            Console.WriteLine("Invalid Input! Please enter a numeric value.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Console.WriteLine("Thank You for Using Expense Tracker.\n");
        }
    }

    public void DisplayExpense()
    {
        if (amount > 0)
        {
            Console.WriteLine("------ Expense Details ------");
            Console.WriteLine("Category : " + category);
            Console.WriteLine("Amount   : ₹" + amount);

            Console.WriteLine("\n------ Expense Summary ------");
            Console.WriteLine("Total Expenses     : ₹" + amount);
            Console.WriteLine("Number of Expenses : 1");
        }
    }
}

class Program
{
    static void Main()
    {
        Expense exp = new Expense();

        exp.GetExpense();

        exp.DisplayExpense();

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}