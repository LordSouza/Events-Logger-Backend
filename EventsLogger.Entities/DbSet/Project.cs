namespace EventsLogger.Entities.DbSet;

public class Project : BaseEntity
{
    public Project()
    {
        UserProjects = new HashSet<UserProject>();

    }
    public string Name { get; set; } = string.Empty;
    public virtual Address? Address { get; set; }
    public ICollection<UserProject> UserProjects { get; set; }
}