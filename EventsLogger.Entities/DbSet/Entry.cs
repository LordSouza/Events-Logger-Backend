using System.ComponentModel.DataAnnotations.Schema;

namespace EventsLogger.Entities.DbSet;

public class Entry : BaseEntity
{
    public Entry()
    {
        FilesUrl = new List<string>();
    }
    public string Description { get; set; } = string.Empty;
    public List<string> FilesUrl { get; set; }

    [ForeignKey("User")]
    public string UserId { get; set; } = string.Empty;
    public required User User { get; set; }
    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    public required Project Project { get; set; }

}