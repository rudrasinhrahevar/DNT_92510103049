using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EventRegistrationPortal
{
    // ==========================================
    // ASP.NET Control & Validation Simulation
    // ==========================================

    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }

    // RequiredFieldValidator
    public static class RequiredFieldValidator
    {
        public static bool Validate(string value, string errorMessage, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result.IsValid = false;
                result.ErrorMessages.Add(errorMessage);
                return false;
            }
            return true;
        }
    }

    // RegularExpressionValidator
    public static class RegularExpressionValidator
    {
        public static bool Validate(string value, string pattern, string errorMessage, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, pattern))
            {
                result.IsValid = false;
                result.ErrorMessages.Add(errorMessage);
                return false;
            }
            return true;
        }
    }

    // RangeValidator
    public static class RangeValidator
    {
        public static bool Validate(int value, int min, int max, string errorMessage, ValidationResult result)
        {
            if (value < min || value > max)
            {
                result.IsValid = false;
                result.ErrorMessages.Add(errorMessage);
                return false;
            }
            return true;
        }
    }

    // CompareValidator
    public static class CompareValidator
    {
        public static bool Validate(string value1, string value2, string errorMessage, ValidationResult result)
        {
            if (value1 != value2)
            {
                result.IsValid = false;
                result.ErrorMessages.Add(errorMessage);
                return false;
            }
            return true;
        }
    }

    // CustomValidator
    public static class CustomValidator
    {
        public static bool Validate(Func<bool> customLogic, string errorMessage, ValidationResult result)
        {
            if (!customLogic())
            {
                result.IsValid = false;
                result.ErrorMessages.Add(errorMessage);
                return false;
            }
            return true;
        }
    }

    // ==========================================
    // Event Data Model & Portal Logic
    // ==========================================

    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Date { get; set; } = "";
        public string Venue { get; set; } = "";
        public double TicketPrice { get; set; }
        public int AvailableSeats { get; set; }
    }

    public class Registration
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public int Age { get; set; }
        public Event SelectedEvent { get; set; } = new Event();
        public int NumberOfTickets { get; set; }
        public string TicketType { get; set; } = "Standard";
        public string PaymentMethod { get; set; } = "";
        public double TotalAmount { get; set; }
        public string RegistrationId { get; set; } = "";
    }

    class Program
    {
        static List<Event> events = new List<Event>
        {
            new Event { Id = 1, Title = "Tech Innovators Summit 2026", Date = "15-Aug-2026", Venue = "Auditorium Hall A", TicketPrice = 1500, AvailableSeats = 50 },
            new Event { Id = 2, Title = "AI & Cloud Computing Workshop", Date = "20-Aug-2026", Venue = "Lab Block 3", TicketPrice = 800, AvailableSeats = 30 },
            new Event { Id = 3, Title = "National Hackathon 2026", Date = "05-Sep-2026", Venue = "Main Convention Center", TicketPrice = 500, AvailableSeats = 100 }
        };

        static void Main()
        {
            Console.WriteLine("=========================================================");
            Console.WriteLine("     ONLINE EVENT REGISTRATION PORTAL (ASP.NET)          ");
            Console.WriteLine("=========================================================\n");

            DisplayAvailableEvents();

            Registration reg = new Registration();
            ValidationResult valResult = new ValidationResult();

            // ASP.NET Form Controls Input & Validation Sequence

            // 1. Full Name (RequiredFieldValidator)
            Console.Write("Enter Full Name: ");
            reg.FullName = Console.ReadLine() ?? "";
            RequiredFieldValidator.Validate(reg.FullName, "[RequiredFieldValidator]: Full Name cannot be empty.", valResult);

            // 2. Email & Confirm Email (RequiredFieldValidator, RegularExpressionValidator, CompareValidator)
            Console.Write("Enter Email Address: ");
            reg.Email = Console.ReadLine() ?? "";
            RequiredFieldValidator.Validate(reg.Email, "[RequiredFieldValidator]: Email Address cannot be empty.", valResult);
            RegularExpressionValidator.Validate(reg.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", "[RegularExpressionValidator]: Invalid Email Format (e.g. user@domain.com).", valResult);

            Console.Write("Confirm Email Address: ");
            string confirmEmail = Console.ReadLine() ?? "";
            CompareValidator.Validate(reg.Email, confirmEmail, "[CompareValidator]: Confirm Email does not match Email Address.", valResult);

            // 3. Mobile Number (RequiredFieldValidator, RegularExpressionValidator)
            Console.Write("Enter 10-Digit Phone Number: ");
            reg.Phone = Console.ReadLine() ?? "";
            RequiredFieldValidator.Validate(reg.Phone, "[RequiredFieldValidator]: Phone number cannot be empty.", valResult);
            RegularExpressionValidator.Validate(reg.Phone, @"^\d{10}$", "[RegularExpressionValidator]: Phone number must contain exactly 10 digits.", valResult);

            // 4. Age (RangeValidator)
            Console.Write("Enter Age (18 - 60): ");
            int.TryParse(Console.ReadLine(), out int age);
            reg.Age = age;
            RangeValidator.Validate(reg.Age, 18, 60, "[RangeValidator]: Age must be between 18 and 60.", valResult);

            // 5. Select Event (CustomValidator)
            Console.Write("Select Event ID (1-3): ");
            int.TryParse(Console.ReadLine(), out int eventId);
            Event? chosenEvent = events.Find(e => e.Id == eventId);
            
            CustomValidator.Validate(() => chosenEvent != null, "[CustomValidator]: Invalid Event Selection.", valResult);

            if (chosenEvent != null)
            {
                reg.SelectedEvent = chosenEvent;

                // 6. Number of Tickets (RangeValidator, CustomValidator for Available Seats)
                Console.Write($"Enter Number of Tickets (1 - 5) [Available: {chosenEvent.AvailableSeats}]: ");
                int.TryParse(Console.ReadLine(), out int tickets);
                reg.NumberOfTickets = tickets;

                RangeValidator.Validate(reg.NumberOfTickets, 1, 5, "[RangeValidator]: You can register between 1 and 5 tickets per transaction.", valResult);
                CustomValidator.Validate(() => tickets <= chosenEvent.AvailableSeats, $"[CustomValidator]: Requested tickets exceed available seats ({chosenEvent.AvailableSeats}).", valResult);
            }

            // 7. Ticket Type DropDownList Control (Standard / VIP)
            Console.WriteLine("\nSelect Ticket Category:");
            Console.WriteLine("1. Standard (Base Price)");
            Console.WriteLine("2. VIP (+50% Premium)");
            Console.Write("Choice (1-2): ");
            string typeChoice = Console.ReadLine() ?? "1";
            if (typeChoice == "2")
            {
                reg.TicketType = "VIP";
            }

            // 8. Payment Method Control
            Console.WriteLine("\nSelect Payment Method:");
            Console.WriteLine("1. Credit / Debit Card");
            Console.WriteLine("2. UPI / Net Banking");
            Console.Write("Choice (1-2): ");
            string payChoice = Console.ReadLine() ?? "1";
            reg.PaymentMethod = (payChoice == "2") ? "UPI / Net Banking" : "Credit / Debit Card";

            // Validation Summary Assessment
            Console.WriteLine("\n=========================================================");
            Console.WriteLine("               ASP.NET VALIDATION SUMMARY                ");
            Console.WriteLine("=========================================================");

            if (!valResult.IsValid)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Validation Failed! Please correct the following errors:\n");
                foreach (var err in valResult.ErrorMessages)
                {
                    Console.WriteLine(" ❌ " + err);
                }
                Console.ResetColor();
                Console.WriteLine("\nRegistration Aborted due to Validation Errors.");
            }
            else
            {
                // Calculate Total Amount
                double basePrice = reg.SelectedEvent.TicketPrice * reg.NumberOfTickets;
                if (reg.TicketType == "VIP")
                {
                    basePrice *= 1.5;
                }
                reg.TotalAmount = basePrice;
                reg.RegistrationId = "EVT-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                // Deduct seats
                reg.SelectedEvent.AvailableSeats -= reg.NumberOfTickets;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" ✅ All ASP.NET Controls Validated Successfully!");
                Console.ResetColor();

                // Display Ticket Invoice / Confirmation
                Console.WriteLine("\n=========================================================");
                Console.WriteLine("             REGISTRATION CONFIRMATION TICKET            ");
                Console.WriteLine("=========================================================");
                Console.WriteLine($" Registration ID : {reg.RegistrationId}");
                Console.WriteLine($" Candidate Name  : {reg.FullName}");
                Console.WriteLine($" Email Address   : {reg.Email}");
                Console.WriteLine($" Phone Number    : {reg.Phone}");
                Console.WriteLine($" Candidate Age   : {reg.Age}");
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine($" Event Title     : {reg.SelectedEvent.Title}");
                Console.WriteLine($" Date & Venue    : {reg.SelectedEvent.Date} | {reg.SelectedEvent.Venue}");
                Console.WriteLine($" Ticket Type     : {reg.TicketType}");
                Console.WriteLine($" Tickets Booked  : {reg.NumberOfTickets}");
                Console.WriteLine($" Total Amount    : ₹{reg.TotalAmount:F2}");
                Console.WriteLine($" Payment Status  : SUCCESSFUL ({reg.PaymentMethod})");
                Console.WriteLine("=========================================================");
                Console.WriteLine("   Thank you for registering! Event details sent to email.");
                Console.WriteLine("=========================================================\n");
            }

            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        static void DisplayAvailableEvents()
        {
            Console.WriteLine("Available Events:");
            Console.WriteLine("----------------------------------------------------------------------------------");
            Console.WriteLine(string.Format("{0,-5} | {1,-35} | {2,-12} | {3,-8} | {4,-6}", "ID", "Event Title", "Date", "Price", "Seats"));
            Console.WriteLine("----------------------------------------------------------------------------------");
            foreach (var evt in events)
            {
                Console.WriteLine(string.Format("{0,-5} | {1,-35} | {2,-12} | ₹{3,-7} | {4,-6}", evt.Id, evt.Title, evt.Date, evt.TicketPrice, evt.AvailableSeats));
            }
            Console.WriteLine("----------------------------------------------------------------------------------\n");
        }
    }
}
