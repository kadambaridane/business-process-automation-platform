using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Data.Configurations;

public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);

        builder.HasOne(w => w.Application)
            .WithMany(a => a.WorkflowDefinitions)
            .HasForeignKey(w => w.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
