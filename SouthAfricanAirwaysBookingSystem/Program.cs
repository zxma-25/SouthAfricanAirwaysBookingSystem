using Microsoft.EntityFrameworkCore;
using SouthAfricanAirwaysApp.Data;
using SouthAfricanAirwaysBookingSystem;

namespace SouthAfricanAirwaysApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.Title = "South African Airways - Flight Booking System";

        using var context = new AppDbContext();
        var bookingService = new FlightBookingService(context);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==================================================================");
        Console.WriteLine("    SOUTH AFRICAN AIRWAYS - ENTERPRISE FLIGHT BOOKING SYSTEM     ");
        Console.WriteLine("==================================================================");
        Console.ResetColor();

        Console.WriteLine("Initializing Database Connection...");
        bool isConnected = await bookingService.EnsureDatabaseConnectedAsync();

        if (!isConnected)
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            return;
        }

        bool keepRunning = true;
        while (keepRunning)
        {
            DisplayMenu();
            Console.Write("\nSelect Menu Option [1-13]: ");
            string? choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    await bookingService.DisplayAllPassengerRecordsAsync();
                    break;

                case "2":
                    Console.Write("Enter Passenger Search Term (Name/Email/Passport): ");
                    string term = Console.ReadLine() ?? "";
                    await bookingService.SearchPassengersAsync(term);
                    break;

                case "3":
                    Console.Write("Enter Departure City or Code (e.g., Johannesburg or JNB): ");
                    string dep = Console.ReadLine() ?? "";
                    await bookingService.DisplayFlightsByDepartureAsync(dep);
                    break;

                case "4":
                    Console.Write("Enter Arrival City or Code (e.g., Cape Town or CPT): ");
                    string arr = Console.ReadLine() ?? "";
                    await bookingService.DisplayFlightsByArrivalAsync(arr);
                    break;

                case "5":
                    await bookingService.DisplayPassengerBookingsAsync();
                    break;

                case "6":
                    Console.Write("Enter Flight ID: ");
                    if (int.TryParse(Console.ReadLine(), out int flightId))
                        await bookingService.DisplayFlightManifestAsync(flightId);
                    else
                        Console.WriteLine("Invalid Flight ID format.");
                    break;

                case "7":
                    await bookingService.DisplayRevenuePerFlightReportAsync();
                    break;

                case "8":
                    await bookingService.DisplayOverallBookingSummaryReportAsync();
                    break;

                case "9":
                    await PromptAndAddNewPassengerAsync(bookingService);
                    break;

                case "10":
                    Console.Write("Enter Passenger ID to Update: ");
                    if (int.TryParse(Console.ReadLine(), out int pid))
                    {
                        Console.Write("Enter New Email Address: ");
                        string newEmail = Console.ReadLine() ?? "";
                        await bookingService.UpdatePassengerEmailAsync(pid, newEmail);
                    }
                    else Console.WriteLine("Invalid Passenger ID.");
                    break;

                case "11":
                    Console.Write("Enter Booking ID to Update: ");
                    if (int.TryParse(Console.ReadLine(), out int bid))
                    {
                        Console.Write("Enter New Status (PENDING / CONFIRMED / CANCELLED): ");
                        string status = Console.ReadLine() ?? "CONFIRMED";
                        await bookingService.UpdateBookingStatusAsync(bid, status);
                    }
                    else Console.WriteLine("Invalid Booking ID.");
                    break;

                case "12":
                    Console.Write("Enter Booking ID to Test FK Delete Constraint: ");
                    if (int.TryParse(Console.ReadLine(), out int delId))
                        await bookingService.DemonstrateDeleteConstraintAsync(delId);
                    else
                        Console.WriteLine("Invalid Booking ID.");
                    break;

                case "13":
                    keepRunning = false;
                    Console.WriteLine("\nThank you for using the South African Airways Booking System.");
                    break;

                default:
                    Console.WriteLine("\nInvalid option selected. Please try again.");
                    break;
            }

            if (keepRunning)
            {
                Console.WriteLine("\nPress ENTER to return to main menu...");
                Console.ReadLine();
            }
        }
    }

    private static void DisplayMenu()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--- MAIN MENU ---");
        Console.ResetColor();
        Console.WriteLine(" 1.  Display All Passengers");
        Console.WriteLine(" 2.  Search Passengers (Name, Email, Passport)");
        Console.WriteLine(" 3.  Display Flights by Departure Location");
        Console.WriteLine(" 4.  Display Flights by Arrival Location");
        Console.WriteLine(" 5.  Display Passenger Itineraries (Multi-Table JOIN)");
        Console.WriteLine(" 6.  Display Flight Passenger Manifest");
        Console.WriteLine(" 7.  Display Revenue Per Flight Report");
        Console.WriteLine(" 8.  Display System Summary Report");
        Console.WriteLine(" 9.  Add New Passenger");
        Console.WriteLine(" 10. Update Passenger Email");
        Console.WriteLine(" 11. Update Booking Status");
        Console.WriteLine(" 12. Demonstrate Foreign Key Delete Constraint Safety");
        Console.WriteLine(" 13. Exit System");
    }

    private static async Task PromptAndAddNewPassengerAsync(FlightBookingService service)
    {
        Console.WriteLine("\n--- Add New Passenger ---");
        Console.Write("First Name: ");
        string firstName = Console.ReadLine() ?? "";

        Console.Write("Last Name: ");
        string lastName = Console.ReadLine() ?? "";

        Console.Write("Email: ");
        string email = Console.ReadLine() ?? "";

        Console.Write("Phone (optional, press Enter to skip): ");
        string? phone = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(phone)) phone = null;

        Console.Write("Passport Number (optional, press Enter to skip): ");
        string? passport = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(passport)) passport = null;

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("First Name, Last Name, and Email are required.");
            return;
        }

        var newPassenger = new Passenger
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            PassportNumber = passport
        };

        await service.AddNewPassengerAsync(newPassenger);
    }
}

public class FlightBookingService
{
    private readonly AppDbContext _context;

    public FlightBookingService(AppDbContext context)
    {
        _context = context;
    }

    // A. Connection Management
    public async Task<bool> EnsureDatabaseConnectedAsync()
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Could not connect to the target database.");
                Console.ResetColor();
                return false;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" Successfully connected to the database.");
            Console.ResetColor();
            return true;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Database Connection Error: {ex.Message}");
            Console.ResetColor();
            return false;
        }
    }

    // B. Data Retrieval & Reporting
    public async Task DisplayAllPassengerRecordsAsync()
    {
        var passengers = await _context.Passengers.AsNoTracking().ToListAsync();

        Console.WriteLine("\n=================================== PASSENGER RECORDS ===================================");
        Console.WriteLine($"{"ID",-5} | {"First Name",-12} | {"Last Name",-12} | {"Email",-28} | {"Phone",-15} | {"Passport",-10}");
        Console.WriteLine(new string('-', 95));

        foreach (var p in passengers)
        {
            Console.WriteLine($"{p.PassengerID,-5} | {p.FirstName,-12} | {p.LastName,-12} | {p.Email,-28} | {p.Phone ?? "N/A",-15} | {p.PassportNumber ?? "N/A",-10}");
        }
        Console.WriteLine("=========================================================================================\n");
    }

    public async Task SearchPassengersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return;

        var term = searchTerm.ToLower();
        var results = await _context.Passengers
            .AsNoTracking()
            .Where(p => p.FirstName.ToLower().Contains(term) ||
                        p.LastName.ToLower().Contains(term) ||
                        p.Email.ToLower().Contains(term) ||
                        (p.PassportNumber != null && p.PassportNumber.ToLower().Contains(term)))
            .ToListAsync();

        Console.WriteLine($"\n--- Search Results for: '{searchTerm}' ---");
        if (!results.Any())
        {
            Console.WriteLine("No matching passenger records found.");
            return;
        }

        foreach (var p in results)
        {
            Console.WriteLine($"ID: {p.PassengerID} | Name: {p.FirstName} {p.LastName} | Email: {p.Email} | Passport: {p.PassportNumber ?? "N/A"}");
        }
    }

    public async Task DisplayFlightsByDepartureAsync(string departureCityOrCode)
    {
        var term = departureCityOrCode.ToLower();
        var flights = await _context.Flights
            .Include(f => f.DepartureAirport)
            .Include(f => f.DestinationAirport)
            .Include(f => f.Aircraft)
            .AsNoTracking()
            .Where(f => f.DepartureAirportID.ToLower() == term || f.DepartureAirport.City.ToLower().Contains(term))
            .ToListAsync();

        Console.WriteLine($"\n--- Flights Departing From: '{departureCityOrCode}' ---");
        PrintFlightTable(flights);
    }

    public async Task DisplayFlightsByArrivalAsync(string arrivalCityOrCode)
    {
        var term = arrivalCityOrCode.ToLower();
        var flights = await _context.Flights
            .Include(f => f.DepartureAirport)
            .Include(f => f.DestinationAirport)
            .Include(f => f.Aircraft)
            .AsNoTracking()
            .Where(f => f.DestinationAirportID.ToLower() == term || f.DestinationAirport.City.ToLower().Contains(term))
            .ToListAsync();

        Console.WriteLine($"\n--- Flights Arriving At: '{arrivalCityOrCode}' ---");
        PrintFlightTable(flights);
    }

    public async Task DisplayPassengerBookingsAsync()
    {
        var tickets = await _context.Tickets
            .Include(t => t.Booking)
                .ThenInclude(b => b.Payments)
            .Include(t => t.Passenger)
            .Include(t => t.Flight)
                .ThenInclude(f => f.DepartureAirport)
            .Include(t => t.Flight)
                .ThenInclude(f => f.DestinationAirport)
            .AsNoTracking()
            .ToListAsync();

        Console.WriteLine("\n====================================== PASSENGER ITINERARIES ======================================");
        foreach (var t in tickets)
        {
            var payment = t.Booking.Payments.FirstOrDefault();

            Console.WriteLine($"Ticket No  : {t.TicketNumber} | PNR: {t.Booking.PnrCode} | Status: {t.Booking.Status}");
            Console.WriteLine($"Passenger  : {t.Passenger.FirstName} {t.Passenger.LastName} ({t.Passenger.Email})");
            Console.WriteLine($"Flight     : {t.Flight.FlightNumber} | {t.Flight.DepartureAirport.City} ({t.Flight.DepartureAirportID}) -> {t.Flight.DestinationAirport.City} ({t.Flight.DestinationAirportID})");
            Console.WriteLine($"Dep Time   : {t.Flight.ScheduledDeparture:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"Seat/Class : Seat {t.SeatNumber} ({t.CabinClass}) | Price: R {t.Price:N2}");
            Console.WriteLine($"Payment    : R {payment?.Amount ?? 0:N2} | Method: {payment?.PaymentMethod ?? "N/A"} | Status: {payment?.PaymentStatus ?? "N/A"}");
            Console.WriteLine(new string('-', 98));
        }
    }

    public async Task DisplayFlightManifestAsync(int flightId)
    {
        var flight = await _context.Flights
            .Include(f => f.DepartureAirport)
            .Include(f => f.DestinationAirport)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FlightID == flightId);

        if (flight == null)
        {
            Console.WriteLine($"\n Flight ID {flightId} not found.");
            return;
        }

        var tickets = await _context.Tickets
            .Where(t => t.FlightID == flightId)
            .Include(t => t.Passenger)
            .Include(t => t.Booking)
            .AsNoTracking()
            .ToListAsync();

        Console.WriteLine($"\n================ MANIFEST FOR FLIGHT {flight.FlightNumber} ({flight.DepartureAirportID} -> {flight.DestinationAirportID}) ================");
        Console.WriteLine($"Total Ticketed Passengers: {tickets.Count}");
        Console.WriteLine(new string('-', 85));

        foreach (var t in tickets)
        {
            Console.WriteLine($"Ticket: {t.TicketNumber} | Passenger: {t.Passenger.FirstName} {t.Passenger.LastName,-18} | Seat: {t.SeatNumber,-5} | Class: {t.CabinClass,-10} | Booking Status: {t.Booking.Status}");
        }
        Console.WriteLine("===================================================================================\n");
    }

    public async Task DisplayRevenuePerFlightReportAsync()
    {
        var revenueData = await _context.Flights
            .AsNoTracking()
            .Select(f => new
            {
                f.FlightID,
                f.FlightNumber,
                Route = f.DepartureAirportID + " -> " + f.DestinationAirportID,
                TotalTickets = f.Tickets.Count,
                TotalRevenue = f.Tickets.Sum(t => (decimal?)t.Price) ?? 0m
            })
            .ToListAsync();

        Console.WriteLine("\n======================== REVENUE REPORT PER FLIGHT ========================");
        Console.WriteLine($"{"Flight No.",-10} | {"Route",-15} | {"Tickets Sold",-14} | {"Total Revenue (ZAR)",-20}");
        Console.WriteLine(new string('-', 68));

        foreach (var r in revenueData)
        {
            Console.WriteLine($"{r.FlightNumber,-10} | {r.Route,-15} | {r.TotalTickets,-14} | R {r.TotalRevenue,17:N2}");
        }
        Console.WriteLine("===========================================================================\n");
    }

    public async Task DisplayOverallBookingSummaryReportAsync()
    {
        var totalPassengers = await _context.Passengers.CountAsync();
        var totalBookings = await _context.Bookings.CountAsync();
        var totalTickets = await _context.Tickets.CountAsync();
        var totalRevenue = await _context.Payments
            .Where(p => p.PaymentStatus == "Completed")
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        Console.WriteLine("\n================ OVERALL SYSTEM SUMMARY REPORT ================");
        Console.WriteLine($" Total Registered Passengers : {totalPassengers}");
        Console.WriteLine($" Total Bookings Created      : {totalBookings}");
        Console.WriteLine($" Total Tickets Issued        : {totalTickets}");
        Console.WriteLine($" Total Revenue Collected     : R {totalRevenue:N2}");
        Console.WriteLine("===============================================================\n");
    }

    // C. Data Modifications
    public async Task AddNewPassengerAsync(Passenger passenger)
    {
        await _context.Passengers.AddAsync(passenger);
        await _context.SaveChangesAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n Passenger '{passenger.FirstName} {passenger.LastName}' added successfully with ID: {passenger.PassengerID}");
        Console.ResetColor();
    }

    public async Task UpdatePassengerEmailAsync(int passengerId, string newEmail)
    {
        var passenger = await _context.Passengers.FindAsync(passengerId);
        if (passenger == null)
        {
            Console.WriteLine($"\n Passenger ID {passengerId} not found.");
            return;
        }

        string oldEmail = passenger.Email;
        passenger.Email = newEmail;
        await _context.SaveChangesAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n Passenger Email Updated Successfully!");
        Console.WriteLine($" Old Email : {oldEmail}");
        Console.WriteLine($" New Email : {passenger.Email}");
        Console.ResetColor();
    }

    public async Task UpdateBookingStatusAsync(int bookingId, string newStatus)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null)
        {
            Console.WriteLine($"\n Booking ID {bookingId} not found.");
            return;
        }

        string oldStatus = booking.Status;
        booking.Status = newStatus;
        await _context.SaveChangesAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n Booking Status Updated Successfully!");
        Console.WriteLine($" Booking ID : {bookingId}");
        Console.WriteLine($" State      : {oldStatus} ---> {newStatus}");
        Console.ResetColor();
    }

    public async Task UpdatePaymentStatusAsync(int paymentId, string newPaymentStatus)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment == null)
        {
            Console.WriteLine($"\n Payment ID {paymentId} not found.");
            return;
        }

        string oldStatus = payment.PaymentStatus;
        payment.PaymentStatus = newPaymentStatus;
        await _context.SaveChangesAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n Payment Status Updated Successfully!");
        Console.WriteLine($" Payment ID : {paymentId}");
        Console.WriteLine($" State      : {oldStatus} ---> {newPaymentStatus}");
        Console.ResetColor();
    }

    // D. Referential Integrity Demonstration
    public async Task DemonstrateDeleteConstraintAsync(int bookingId)
    {
        Console.WriteLine($"\n--- DEMONSTRATION: Attempting to Delete Booking ID {bookingId} ---");

        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null)
        {
            Console.WriteLine($"Booking ID {bookingId} does not exist in the database.");
            return;
        }

        try
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            Console.WriteLine("Deletion completed.");
        }
        catch (DbUpdateException ex)
        {
            _context.Entry(booking).State = EntityState.Unchanged;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[FOREIGN KEY CONSTRAINT VIOLATION INTERCEPTED]");
            Console.WriteLine($"Error Details: {ex.InnerException?.Message ?? ex.Message}");
            Console.WriteLine("\n[EXPLANATION]");
            Console.WriteLine($" Deleting 'Booking' (ID: {bookingId}) failed because dependent records reference it in 'dbo.Ticket'.");
            Console.WriteLine(" In our DbContext model, Ticket foreign keys prohibit cascading deletes to protect ticket records.");
            Console.ResetColor();
        }
    }

    private static void PrintFlightTable(List<Flight> flights)
    {
        if (!flights.Any())
        {
            Console.WriteLine("No flights found matching criteria.");
            return;
        }

        Console.WriteLine($"{"ID",-5} | {"Flight No.",-10} | {"Departure",-18} | {"Arrival",-18} | {"Aircraft",-18} | {"Status",-10}");
        Console.WriteLine(new string('-', 88));

        foreach (var f in flights)
        {
            Console.WriteLine($"{f.FlightID,-5} | {f.FlightNumber,-10} | {f.DepartureAirport.City} ({f.DepartureAirportID}),-18 | {f.DestinationAirport.City} ({f.DestinationAirportID}),-18 | {f.Aircraft.Model,-18} | {f.Status,-10}");
        }
    }
}