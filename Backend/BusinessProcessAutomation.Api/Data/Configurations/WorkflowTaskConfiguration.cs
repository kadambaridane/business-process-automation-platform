using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Data.Configurations;

public class WorkflowTaskConfiguration : IEntityTypeConfiguration<WorkflowTask>
{
    public void Configure(EntityTypeBuilder<WorkflowTask> builder)
    {
        builder.ToTable("Tasks");
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);

        // Restrict here: the instance -> stepInstance -> task chain already cascades;
        // this direct FK would otherwise create a second cascade path SQL Server rejects.
        builder.HasOne(t => t.WorkflowInstance)
            .WithMany(i => i.Tasks)
            .HasForeignKey(t => t.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.WorkflowStepInstance)
            .WithOne(si => si.Task)
            .HasForeignKey<WorkflowTask>(t => t.WorkflowStepInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.AssignedToUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedRole)
            .WithMany()
            .HasForeignKey(t => t.AssignedRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.WorkflowStepInstanceId).IsUnique();
        builder.HasIndex(t => new { t.AssignedToUserId, t.Status });
    }
}
