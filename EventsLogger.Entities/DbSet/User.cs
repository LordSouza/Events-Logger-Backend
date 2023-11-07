namespace EventsLogger.Entities.DbSet;

public class User : BaseEntity
{
    public User()
    {
        Entries = new HashSet<Entry>();
        UserProjects = new HashSet<UserProject>();
    }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
    public virtual ICollection<Entry> Entries { get; set; }
    public virtual ICollection<UserProject> UserProjects { get; set; }


}