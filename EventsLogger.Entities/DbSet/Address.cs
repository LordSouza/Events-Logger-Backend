using System.ComponentModel.DataAnnotations.Schema;

namespace EventsLogger.Entities.DbSet;

public class Address : BaseEntity
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    public virtual required Project Project { get; set; }
}