using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;


namespace EventsLogger.Entities.DbSet;

public class User : IdentityUser
{
    public User()
    {
        Entries = new HashSet<Entry>();
        UserProjects = new HashSet<UserProject>();
    }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    public int Status { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
    public ICollection<Entry> Entries { get; set; }
    public ICollection<UserProject> UserProjects { get; set; }


}