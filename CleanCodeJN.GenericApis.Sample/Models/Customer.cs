using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Models;

public class Customer : IEntity<int>
{
    public int Id { get; set; }

    public string Name { get; set; }
}
