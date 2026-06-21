using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.HasIndex(x => new { x.StudentId, x.CourseId }).IsUnique();
        builder.HasOne(x => x.Student).WithMany(x => x.Reviews).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Course).WithMany(x => x.Reviews).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
    }
}
