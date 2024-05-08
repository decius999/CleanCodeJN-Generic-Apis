using CleanCodeJN.GenericApis.Sample.Models;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CleanCodeJN.GenericApis.Sample.Context;

public class MyDbContext : DbContext, IDataContext
{
    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase("MyDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasData(new Customer
        {
            Id = 1,
            Name = "aaa",
        },
        new Customer
        {
            Id = 2,
            Name = "aab",
        },
        new Customer
        {
            Id = 3,
            Name = "aac",
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
