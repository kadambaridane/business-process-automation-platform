using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Data.Configurations;

public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.HasOne(i => i.WorkflowDefinition)
            .WithMany(w => w.WorkflowInstances)
            .HasForeignKey(i => i.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Application)
            .WithMany(a => a.WorkflowInstances)
            .HasForeignKey(i => i.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.FormSubmission)
            .WithOne(s => s.WorkflowInstance)
            .HasForeignKey<WorkflowInstance>(i => i.FormSubmissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.SubmittedByUser)
            .WithMany()
            .HasForeignKey(i => i.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.CurrentStep)
            .WithMany()
            .HasForeignKey(i => i.CurrentStepId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.FormSubmissionId).IsUnique();
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.ApplicationId);
    }
}
