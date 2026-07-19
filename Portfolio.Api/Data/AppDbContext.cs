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
        public DbSet<ProjectTag> Tags => Set<ProjectTag>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>(p =>
            {
                p.HasKey(x => x.Id);

                p.Property(x => x.Id)
                  .ValueGeneratedNever();

                p.Property(x => x.Title)
                  .IsRequired()
                  .HasMaxLength(Project.MaxTitleLength);
            });

            modelBuilder.Entity<ProjectImage>(pi =>
            {
                pi.HasKey(x => x.Id);

                pi.Property(x => x.Id)
                    .ValueGeneratedNever();

                pi.HasOne(x => x.Project)
                  .WithMany(x => x.Images)
                  .HasForeignKey(x => x.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

                pi.Property(x => x.FullKey).IsRequired().HasMaxLength(512);
                pi.Property(x => x.ThumbKey).IsRequired().HasMaxLength(512);
                pi.Property(x => x.ClientId).HasMaxLength(ProjectImage.MaxClientIdLength);

                pi.HasIndex(x => x.ProjectId);
                pi.HasIndex(x => new { x.ProjectId, x.SortOrder });
                pi.HasIndex(x => new { x.ProjectId, x.ClientId })
                  .IsUnique()
                  .HasFilter("[ClientId] IS NOT NULL");
            });

            modelBuilder.Entity<ProjectTag>(t =>
            {
                t.HasKey(x => x.Id);
                t.Property(x => x.Id).ValueGeneratedNever();
                t.Property(x => x.Name).IsRequired().HasMaxLength(50);
                t.Property(x => x.Value).IsRequired().HasMaxLength(50);
                t.HasIndex(x => x.Value).IsUnique();
            });

            modelBuilder.Entity<Project>()
                .HasMany(x => x.Tags)
                .WithMany(x => x.Projects)
                .UsingEntity(j => j.ToTable("ProjectTags"));

            modelBuilder.Entity<ProjectLink>(pl =>
            {
                pl.HasKey(x => x.Id);

                pl.Property(x => x.Id)
                  .ValueGeneratedNever();

                pl.HasOne(x => x.Project)
                  .WithMany(x => x.Links)
                  .HasForeignKey(x => x.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

                pl.Property(x => x.Url).IsRequired().HasMaxLength(ProjectLink.MaxUrlLength);
                pl.Property(x => x.LinkText).IsRequired().HasMaxLength(ProjectLink.MaxLinkTextLength);
                pl.Property(x => x.LinkType).IsRequired();
                pl.Property(x => x.SortOrder).IsRequired();

                pl.HasIndex(x => x.ProjectId);
                pl.HasIndex(x => new { x.ProjectId, x.SortOrder });
            });
        }
    }
}
