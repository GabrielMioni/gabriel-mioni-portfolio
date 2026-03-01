using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Domain.Projects;

namespace Portfolio.Api.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Project> Projects => Set<Project>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>(p =>
            {
                p.Property(p => p.Title).HasMaxLength(300);
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
        }
    }
}
