using Microsoft.EntityFrameworkCore;
using SouthAfricanAirwaysBookingSystem;

namespace SouthAfricanAirwaysApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Passenger> Passengers { get; set; } = null!;
    public DbSet<Airport> Airports { get; set; } = null!;
    public DbSet<Aircraft> Aircrafts { get; set; } = null!;
    public DbSet<Flight> Flights { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Adjust connection string as needed for your database
            optionsBuilder.UseSqlServer("Server=(localdb)\\SAAirwaysBooking.db;Database=SAABookingSystem;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Foreign Keys & Relationships
        modelBuilder.Entity<Flight>()
            .HasOne(f => f.DepartureAirport)
            .WithMany(a => a.DepartingFlights)
            .HasForeignKey(f => f.DepartureAirportID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Flight>()
            .HasOne(f => f.DestinationAirport)
            .WithMany(a => a.ArrivingFlights)
            .HasForeignKey(f => f.DestinationAirportID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Booking)
            .WithMany(b => b.Tickets)
            .HasForeignKey(t => t.BookingID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Passenger)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.PassengerID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Flight)
            .WithMany(f => f.Tickets)
            .HasForeignKey(t => t.FlightID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Booking)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BookingID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}