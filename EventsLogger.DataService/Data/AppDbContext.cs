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
            modelBuilder.Entity<Entry>(entity =>
            {
                entity.HasKey(r => new { r.UserId, r.ProjectId });
            });
            modelBuilder.Entity<UserProject>().HasKey(
                r => new { r.UserId, r.ProjectId }
                );
        }

    }

}