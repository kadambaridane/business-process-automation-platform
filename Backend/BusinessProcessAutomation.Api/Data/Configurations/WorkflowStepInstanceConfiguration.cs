using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Data.Configurations;

public class WorkflowStepInstanceConfiguration : IEntityTypeConfiguration<WorkflowStepInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowStepInstance> builder)
    {
        builder.HasOne(si => si.WorkflowInstance)
            .WithMany(i => i.WorkflowStepInstances)
            .HasForeignKey(si => si.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(si => si.WorkflowStep)
            .WithMany(s => s.WorkflowStepInstances)
            .HasForeignKey(si => si.WorkflowStepId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
