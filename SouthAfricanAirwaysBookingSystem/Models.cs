using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SouthAfricanAirwaysBookingSystem;

[Table("Passenger", Schema = "dbo")]
public class Passenger
{
    [Key]
    [Column("passengerID")]
    public int PassengerID { get; set; }

    [Required]
    [Column("firstName", TypeName = "varchar(50)")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Column("lastName", TypeName = "varchar(50)")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Column("email", TypeName = "varchar(100)")]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Column("phone", TypeName = "varchar(20)")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Column("passportNumber", TypeName = "varchar(20)")]
    [StringLength(20)]
    public string? PassportNumber { get; set; }


    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}

[Table("Airport", Schema = "dbo")]
public class Airport
{
    [Key]
    [Column("airportID", TypeName = "char(3)")]
    [StringLength(3)]
    public string AirportID { get; set; } = string.Empty;

    [Required]
    [Column("airportName", TypeName = "varchar(100)")]
    [StringLength(100)]
    public string AirportName { get; set; } = string.Empty;

    [Required]
    [Column("city", TypeName = "varchar(100)")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [Column("country", TypeName = "varchar(100)")]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [InverseProperty(nameof(Flight.DepartureAirport))]
    public virtual ICollection<Flight> DepartingFlights { get; set; } = new List<Flight>();

    [InverseProperty(nameof(Flight.DestinationAirport))]
    public virtual ICollection<Flight> ArrivingFlights { get; set; } = new List<Flight>();
}

[Table("Flight", Schema = "dbo")]
public class Flight
{
    [Key]
    [Column("flightID")]
    public int FlightID { get; set; }

    [Required]
    [Column("flightNumber", TypeName = "varchar(10)")]
    [StringLength(10)]
    public string FlightNumber { get; set; } = string.Empty;

    [Required]
    [Column("departureAirportID", TypeName = "char(3)")]
    [StringLength(3)]
    public string DepartureAirportID { get; set; } = string.Empty;

    [Required]
    [Column("destinationAirportID", TypeName = "char(3)")]
    [StringLength(3)]
    public string DestinationAirportID { get; set; } = string.Empty;

    [Required]
    [Column("scheduledDeparture", TypeName = "datetime2")]
    public DateTime ScheduledDeparture { get; set; }

    [Required]
    [Column("scheduledArrival", TypeName = "datetime2")]
    public DateTime ScheduledArrival { get; set; }

    [Required]
    [Column("aircraftID")]
    public int AircraftID { get; set; }

    [Required]
    [Column("status", TypeName = "varchar(20)")]
    [StringLength(20)]
    public string Status { get; set; } = "SCHEDULED";

    [ForeignKey(nameof(DepartureAirportID))]
    public virtual Airport DepartureAirport { get; set; } = null!;

    [ForeignKey(nameof(DestinationAirportID))]
    public virtual Airport DestinationAirport { get; set; } = null!;

    [ForeignKey(nameof(AircraftID))]
    public virtual Aircraft Aircraft { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}

[Table("Booking", Schema = "dbo")]
public class Booking
{
    [Key]
    [Column("bookingID")]
    public int BookingID { get; set; }

    [Required]
    [Column("pnrCode", TypeName = "varchar(6)")]
    [StringLength(6)]
    public string PnrCode { get; set; } = string.Empty;

    [Required]
    [Column("bookingDate", TypeName = "datetime2")]
    public DateTime BookingDate { get; set; } = DateTime.Now;

    [Required]
    [Column("status", TypeName = "varchar(20)")]
    [StringLength(20)]
    public string Status { get; set; } = "CONFIRMED";

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

[Table("Ticket", Schema = "dbo")]
public class Ticket
{
    [Key]
    [Column("ticketNumber", TypeName = "varchar(13)")]
    [StringLength(13)]
    public string TicketNumber { get; set; } = string.Empty;

    [Required]
    [Column("bookingID")]
    public int BookingID { get; set; }

    [Required]
    [Column("passengerID")]
    public int PassengerID { get; set; }

    [Required]
    [Column("flightID")]
    public int FlightID { get; set; }

    [Required]
    [Column("seatNumber", TypeName = "varchar(5)")]
    [StringLength(5)]
    public string SeatNumber { get; set; } = string.Empty;

    [Required]
    [Column("cabinClass", TypeName = "varchar(20)")]
    [StringLength(20)]
    public string CabinClass { get; set; } = "Economy";

    [Required]
    [Column("price", TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    [Required]
    [Column("issuedDate", TypeName = "datetime2")]
    public DateTime IssuedDate { get; set; } = DateTime.Now;

    [ForeignKey(nameof(BookingID))]
    public virtual Booking Booking { get; set; } = null!;

    [ForeignKey(nameof(PassengerID))]
    public virtual Passenger Passenger { get; set; } = null!;

    [ForeignKey(nameof(FlightID))]
    public virtual Flight Flight { get; set; } = null!;
}

[Table("Payment", Schema = "dbo")]
public class Payment
{
    [Key]
    [Column("PaymentID")]
    public int PaymentID { get; set; }

    [Required]
    [Column("BookingID")]
    public int BookingID { get; set; }

    [Required]
    [Column("Amount", TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    [Required]
    [Column("PaymentDate", TypeName = "datetime")]
    public DateTime PaymentDate { get; set; } = DateTime.Now;

    [Required]
    [Column("PaymentMethod", TypeName = "nvarchar(50)")]
    [StringLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required]
    [Column("PaymentStatus", TypeName = "nvarchar(50)")]
    [StringLength(50)]
    public string PaymentStatus { get; set; } = "Pending";

    [ForeignKey(nameof(BookingID))]
    public virtual Booking Booking { get; set; } = null!;
}
[Table("Aircraft", Schema = "dbo")]
public class Aircraft
{
    [Key]
    [Column("aircraftID")]
    public int AircraftID { get; set; }

    [Required]
    [Column("model", TypeName = "varchar(50)")]
    [StringLength(50)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [Column("totalSeats")]
    public int TotalSeats { get; set; }

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
