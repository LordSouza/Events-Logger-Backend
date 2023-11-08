using EventsLogger.Entities.DbSet;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventsLogger.DataService.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public override DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Entry> Entrys { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<UserProject> UsersProjects { get; set; }


        public AppDbContext(DbContextOptions options) : base(options) { }
        // especified the relationship
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Entry>(entity =>
            {
                entity.HasOne(d => d.User);
                entity.HasOne(d => d.Project);
            });
            modelBuilder.Entity<UserProject>(entity =>
            {
                entity.HasOne(d => d.User);
                entity.HasOne(d => d.Project);
            });
        }

    }

}