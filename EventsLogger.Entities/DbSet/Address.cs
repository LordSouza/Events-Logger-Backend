using System.ComponentModel.DataAnnotations.Schema;

namespace EventsLogger.Entities.DbSet;

public class Address : BaseEntity
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    public required Project Project { get; set; }
}