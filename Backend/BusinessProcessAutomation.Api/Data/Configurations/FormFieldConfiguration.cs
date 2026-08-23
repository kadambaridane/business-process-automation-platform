using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Data.Configurations;

public class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> builder)
    {
        builder.Property(f => f.FieldName).IsRequired().HasMaxLength(100);
        builder.Property(f => f.Label).IsRequired().HasMaxLength(200);

        builder.HasOne(f => f.FormDefinition)
            .WithMany(fd => fd.FormFields)
            .HasForeignKey(f => f.FormDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.FormDefinitionId, f.FieldName }).IsUnique();
    }
}
