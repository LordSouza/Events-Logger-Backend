using EventsLogger.Entities.DbSet;
using Microsoft.EntityFrameworkCore;

namespace EventsLogger.DataService.Data
{
    public class AppDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<Entry> Entrys { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<UserProject> UsersProjects { get; set; }


        public AppDbContext(DbContextOptions options) : base(options) { }
        // especified the relationship
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(entity =>
            {
                // 1 user - many entries
                entity
                .HasMany(d => d.Entries)
                .WithOne(p => p.User)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_User_Entry")
                ;
                // many users - many projects
                entity
                .HasMany(d => d.UserProjects)
                .WithOne(p => p.User);
            });
            modelBuilder.Entity<Project>(entity =>
            {
                // many users - many projects
                entity
                .HasMany(d => d.UserProjects)
                .WithOne(p => p.Project);
            });
            modelBuilder.Entity<Entry>().HasKey(
                r => new { r.UserId, r.ProjectId }
                );
            modelBuilder.Entity<UserProject>().HasKey(
                r => new { r.UserId, r.ProjectId }
                );
        }

    }

}