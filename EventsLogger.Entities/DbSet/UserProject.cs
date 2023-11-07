using System.ComponentModel.DataAnnotations.Schema;

namespace EventsLogger.Entities.DbSet;

public class UserProject : BaseEntity
{
    [ForeignKey("User")]
    public string UserId { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public User? User { get; set; }
    public Project? Project { get; set; }


    public string Role { get; set; } = string.Empty;
}