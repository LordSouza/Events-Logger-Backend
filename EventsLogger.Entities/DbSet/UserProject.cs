using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EventsLogger.Entities.DbSet;

public class UserProject : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public Guid ProjectId { get; set; }

    public User? User { get; set; }

    [ForeignKey("ProjectId")]
    public Project? Project { get; set; }


    public string Role { get; set; } = string.Empty;
}