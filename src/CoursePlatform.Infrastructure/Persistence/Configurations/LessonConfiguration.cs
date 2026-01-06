using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Infrastructure.Persistence.Configurations
{
    public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            builder.Property(t => t.Title)
                .HasMaxLength(200)
                .IsRequired();

            // Unique Index: CourseId + Order
            // This ensures no two lessons can have the same order in the same course
            builder.HasIndex(l => new { l.CourseId, l.Order })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false"); // Optional: Only for active lessons if needed, but strict unique is safer
        }
    }
}
