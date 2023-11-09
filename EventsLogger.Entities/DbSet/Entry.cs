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

    public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")]
    public required User User { get; set; }
    public Guid ProjectId { get; set; }
    [ForeignKey("ProjectId")]
    public required Project Project { get; set; }

}