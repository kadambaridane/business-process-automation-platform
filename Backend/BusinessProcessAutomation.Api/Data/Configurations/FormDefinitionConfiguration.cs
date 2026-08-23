using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Data.Configurations;

public class FormDefinitionConfiguration : IEntityTypeConfiguration<FormDefinition>
{
    public void Configure(EntityTypeBuilder<FormDefinition> builder)
    {
        builder.Property(f => f.Name).IsRequired().HasMaxLength(200);

        builder.HasOne(f => f.Application)
            .WithMany(a => a.FormDefinitions)
            .HasForeignKey(f => f.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
