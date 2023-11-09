using System.ComponentModel.DataAnnotations.Schema;

namespace EventsLogger.Entities.DbSet;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    [ForeignKey("Address")]
    public Guid AddressId { get; set; }
    public Address? Address { get; set; }
}