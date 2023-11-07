namespace EventsLogger.Entities.DbSet;

public class UserProject : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProjectId { get; set; }
    public virtual User? User { get; set; }
    public virtual Project? Project { get; set; }


    public string Role { get; set; } = string.Empty;
}