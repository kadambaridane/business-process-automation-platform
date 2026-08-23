using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Data.Configurations;

public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.SystemActionKey).HasMaxLength(100);

        builder.HasOne(s => s.WorkflowDefinition)
            .WithMany(w => w.WorkflowSteps)
            .HasForeignKey(s => s.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.AssigneeRole)
            .WithMany()
            .HasForeignKey(s => s.AssigneeRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Not unique: reordering must be able to swap two steps' StepOrder values in one
        // SaveChanges batch, which a unique constraint on this pair makes impossible for EF Core
        // to sequence (each individual UPDATE could momentarily collide with the other row).
        builder.HasIndex(s => new { s.WorkflowDefinitionId, s.StepOrder });
    }
}
