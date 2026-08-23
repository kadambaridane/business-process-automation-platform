using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(r => r.Name).IsUnique();

        builder.HasData(
            new Role { Id = 1, Name = "Admin", Description = "Full administrative access" },
            new Role { Id = 2, Name = "Business User", Description = "Creates and configures business applications, forms, and workflows" },
            new Role { Id = 3, Name = "Employee", Description = "Submits forms and completes assigned tasks" },
            new Role { Id = 4, Name = "Manager", Description = "Approves requests within their team" },
            new Role { Id = 5, Name = "Approver", Description = "General-purpose approval role for workflow steps" }
        );
    }
}
