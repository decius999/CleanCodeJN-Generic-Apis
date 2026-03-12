using CleanCodeJN.GenericApis.Sample.Domain;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CleanCodeJN.GenericApis.Sample.DataAccess;

/// <summary>
/// Entity Framework database context for the sample application, providing access to customers and invoices.
/// </summary>
/// <param name="configuration">The application configuration used to resolve the connection string.</param>
public class MyDbContext(IConfiguration configuration) : DbContext, IDataContext
{
    /// <summary>
    /// Gets or sets the database set of customer entities.
    /// </summary>
    public virtual DbSet<Customer> Customers { get; set; }

    /// <summary>
    /// Gets or sets the database set of invoice entities.
    /// </summary>
    public virtual DbSet<Invoice> Invoices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().OwnsOne(p => p.AddressInfo).HasData(new
        {
            Id = 1,
            CustomerId = 1,
            Street = $"Street_1",
            HouseNo = $"HouseNo_1",
            Zip = "Zip_1",
            City = "City_1",
        });

        modelBuilder.Entity<Customer>().OwnsOne(p => p.AddressInfo).HasData(new
        {
            Id = 2,
            CustomerId = 2,
            Street = $"Street_2",
            HouseNo = $"HouseNo_2",
            Zip = "Zip_2",
            City = "City_2",
        });

        modelBuilder.Entity<Customer>().OwnsOne(p => p.AddressInfo).HasData(new
        {
            Id = 3,
            CustomerId = 3,
            Street = $"Street_3",
            HouseNo = $"HouseNo_3",
            Zip = "Zip_3",
            City = "City_3",
        });

        modelBuilder.Entity<Customer>().OwnsOne(p => p.AddressInfo).HasData(new
        {
            Id = 4,
            CustomerId = 4,
            Street = string.Empty,
            HouseNo = string.Empty,
            Zip = string.Empty,
            City = string.Empty,
        });

        modelBuilder.Entity<Customer>().HasData(new Customer
        {
            Id = 1,
            Name = "Customer_1",
        },
        new Customer
        {
            Id = 2,
            Name = "Customer_2",
        },
        new Customer
        {
            Id = 3,
            Name = "Customer_3",
        },
        new Customer
        {
            Id = 4,
            Name = "Customer_4",
        });

        modelBuilder.Entity<Invoice>().HasData(new Invoice
        {
            Id = Guid.NewGuid(),
            CustomerId = 1,
            Amount = 123.89m,
        },
        new Invoice
        {
            Id = Guid.NewGuid(),
            CustomerId = 1,
            Amount = 1243.42m,
        });
    }
}
