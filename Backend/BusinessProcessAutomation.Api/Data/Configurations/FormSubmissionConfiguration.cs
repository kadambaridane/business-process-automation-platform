using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Data.Configurations;

public class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> builder)
    {
        builder.Property(s => s.SubmissionData).IsRequired();

        builder.HasOne(s => s.FormDefinition)
            .WithMany(fd => fd.FormSubmissions)
            .HasForeignKey(s => s.FormDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Application)
            .WithMany(a => a.FormSubmissions)
            .HasForeignKey(s => s.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SubmittedByUser)
            .WithMany()
            .HasForeignKey(s => s.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.SubmittedDate);
    }
}
