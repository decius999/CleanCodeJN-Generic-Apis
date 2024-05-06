using CleanCodeJN.GenericApis.Sample.Models;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CleanCodeJN.GenericApis.Sample.Context;

public class MyDbContext : DbContext, IDataContext
{
    public virtual DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase("MyDatabase");
}
