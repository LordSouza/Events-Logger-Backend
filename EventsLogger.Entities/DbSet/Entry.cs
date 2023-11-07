namespace EventsLogger.Entities.DbSet;

public class Entry : BaseEntity
{
    public Entry()
    {
        FilesUrl = new List<string>();
    }
    public string Description { get; set; } = string.Empty;
    public List<string> FilesUrl { get; set; }

    public virtual Guid UserId { get; set; }
    public virtual required User User { get; set; }
    public virtual Guid ProjectId { get; set; }
    public virtual required Project Project { get; set; }

}