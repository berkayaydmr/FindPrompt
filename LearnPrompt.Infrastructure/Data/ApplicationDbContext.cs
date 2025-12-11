using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LearnPrompt.Domain.Entities;

namespace LearnPrompt.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<LessonTopic> LessonTopics { get; set; } = null!;
        public DbSet<CourseFile> CourseFiles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Course>()
                .HasMany(c => c.Topics)
                .WithOne(t => t.Course)
                .HasForeignKey(t => t.CourseId);

            builder.Entity<Course>()
                .HasMany(c => c.Files)
                .WithOne(f => f.Course)
                .HasForeignKey(f => f.CourseId);
        }
    }
}
