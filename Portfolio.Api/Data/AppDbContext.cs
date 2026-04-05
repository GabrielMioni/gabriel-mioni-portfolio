using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
        public DbSet<ProjectLink> ProjectLinks => Set<ProjectLink>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>(p =>
            {
                p.Property(x => x.Title).IsRequired().HasMaxLength(300);
            });

            modelBuilder.Entity<ProjectImage>(pi =>
            {
                pi.HasOne(x => x.Project)
                  .WithMany(x => x.Images)
                  .HasForeignKey(x => x.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

                pi.Property(x => x.FullKey).IsRequired().HasMaxLength(512);
                pi.Property(x => x.ThumbKey).IsRequired().HasMaxLength(512);

                pi.HasIndex(x => x.ProjectId);
                pi.HasIndex(x => new { x.ProjectId, x.SortOrder });
            });

            modelBuilder.Entity<ProjectLink>(pl =>
            {
                pl.HasOne(x => x.Project)
                  .WithMany(x => x.Links)
                  .HasForeignKey(x => x.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

                pl.Property(x => x.Link).IsRequired().HasMaxLength(2048);
                pl.Property(x => x.LinkText).IsRequired().HasMaxLength(300);
                pl.Property(x => x.LinkType).IsRequired();
                pl.Property(x => x.SortOrder).IsRequired();

                pl.HasIndex(x => x.ProjectId);
                pl.HasIndex(x => new { x.ProjectId, x.SortOrder });
            });
        }
    }
}