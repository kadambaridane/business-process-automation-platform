using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.DTOs.Forms;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;
using BusinessProcessAutomation.Api.Services;
using Xunit;

namespace BusinessProcessAutomation.Api.Tests.Forms;

public class FormSubmissionServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private record Fixture(AppDbContext Db, IFormSubmissionService Service, int FormId, int UserId);

    /// <summary>Seeds a form with one field of every validated type: required Text (2-50 chars),
    /// required Number (1-30), required Email, required Dropdown (Sick/Vacation/Personal), optional Checkbox.
    /// No active WorkflowDefinition exists, so the engine no-ops and these tests exercise only validation.</summary>
    private static async Task<Fixture> SeedAsync(bool formActive = true)
    {
        var db = CreateDbContext();
        var notificationService = new NotificationService(db);
        var engine = new WorkflowEngineService(db, notificationService, Array.Empty<ISystemTaskHandler>());
        var service = new FormSubmissionService(db, engine);

        var user = new User { FullName = "Requester", Email = "req@test.com", Username = "requester", PasswordHash = "x" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var application = new BusinessApplication { Name = "Leave Request", CreatedByUserId = user.Id };
        db.Applications.Add(application);
        await db.SaveChangesAsync();

        var form = new FormDefinition { ApplicationId = application.Id, Name = "Leave Form", IsActive = formActive };
        db.FormDefinitions.Add(form);
        await db.SaveChangesAsync();

        db.FormFields.AddRange(
            new FormField
            {
                FormDefinitionId = form.Id, FieldName = "employeeName", Label = "Employee Name",
                FieldType = FieldType.Text, IsRequired = true, DisplayOrder = 1,
                ValidationRulesJson = JsonSerializer.Serialize(new FieldValidationRulesDto { MinLength = 2, MaxLength = 50 })
            },
            new FormField
            {
                FormDefinitionId = form.Id, FieldName = "daysRequested", Label = "Days Requested",
                FieldType = FieldType.Number, IsRequired = true, DisplayOrder = 2,
                ValidationRulesJson = JsonSerializer.Serialize(new FieldValidationRulesDto { Min = 1, Max = 30 })
            },
            new FormField
            {
                FormDefinitionId = form.Id, FieldName = "email", Label = "Email",
                FieldType = FieldType.Email, IsRequired = true, DisplayOrder = 3
            },
            new FormField
            {
                FormDefinitionId = form.Id, FieldName = "leaveType", Label = "Leave Type",
                FieldType = FieldType.Dropdown, IsRequired = true, DisplayOrder = 4,
                ValidationRulesJson = JsonSerializer.Serialize(new FieldValidationRulesDto { Options = new List<string> { "Sick", "Vacation", "Personal" } })
            },
            new FormField
            {
                FormDefinitionId = form.Id, FieldName = "urgent", Label = "Urgent",
                FieldType = FieldType.Checkbox, IsRequired = false, DisplayOrder = 5
            }
        );
        await db.SaveChangesAsync();

        return new Fixture(db, service, form.Id, user.Id);
    }

    private static Dictionary<string, JsonElement> Values(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
    }

    private static SubmitFormRequestDto ValidPayload() => new()
    {
        Values = Values(new { employeeName = "Jane Doe", daysRequested = 5, email = "jane@example.com", leaveType = "Vacation" })
    };

    [Fact]
    public async Task SubmitAsync_WithValidData_Succeeds()
    {
        var fixture = await SeedAsync();

        var result = await fixture.Service.SubmitAsync(fixture.FormId, ValidPayload(), fixture.UserId);

        Assert.True(result.Id > 0);
        Assert.Contains("Jane Doe", result.SubmissionData);
    }

    [Fact]
    public async Task SubmitAsync_MissingRequiredField_ThrowsWithFieldLabelInMessage()
    {
        var fixture = await SeedAsync();
        var payload = new SubmitFormRequestDto { Values = Values(new { daysRequested = 5, email = "jane@example.com", leaveType = "Vacation" }) };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.SubmitAsync(fixture.FormId, payload, fixture.UserId));
        Assert.Contains("Employee Name", ex.Message);
        Assert.Contains("required", ex.Message);
    }

    [Fact]
    public async Task SubmitAsync_InvalidEmail_ThrowsArgumentException()
    {
        var fixture = await SeedAsync();
        var payload = new SubmitFormRequestDto
        {
            Values = Values(new { employeeName = "Jane Doe", daysRequested = 5, email = "not-an-email", leaveType = "Vacation" })
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.SubmitAsync(fixture.FormId, payload, fixture.UserId));
        Assert.Contains("valid email", ex.Message);
    }

    [Fact]
    public async Task SubmitAsync_NumberAboveMax_ThrowsArgumentException()
    {
        var fixture = await SeedAsync();
        var payload = new SubmitFormRequestDto
        {
            Values = Values(new { employeeName = "Jane Doe", daysRequested = 999, email = "jane@example.com", leaveType = "Vacation" })
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.SubmitAsync(fixture.FormId, payload, fixture.UserId));
        Assert.Contains("at most 30", ex.Message);
    }

    [Fact]
    public async Task SubmitAsync_DropdownValueNotInOptions_ThrowsArgumentException()
    {
        var fixture = await SeedAsync();
        var payload = new SubmitFormRequestDto
        {
            Values = Values(new { employeeName = "Jane Doe", daysRequested = 5, email = "jane@example.com", leaveType = "Bereavement" })
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.SubmitAsync(fixture.FormId, payload, fixture.UserId));
        Assert.Contains("configured options", ex.Message);
    }

    [Fact]
    public async Task SubmitAsync_TextBelowMinLength_ThrowsArgumentException()
    {
        var fixture = await SeedAsync();
        var payload = new SubmitFormRequestDto
        {
            Values = Values(new { employeeName = "J", daysRequested = 5, email = "jane@example.com", leaveType = "Vacation" })
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.SubmitAsync(fixture.FormId, payload, fixture.UserId));
        Assert.Contains("at least 2 characters", ex.Message);
    }

    [Fact]
    public async Task SubmitAsync_MultipleInvalidFields_ReportsAllErrorsTogether()
    {
        var fixture = await SeedAsync();
        var payload = new SubmitFormRequestDto
        {
            Values = Values(new { employeeName = "Jane Doe", daysRequested = 999, email = "bad-email", leaveType = "Nope" })
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.SubmitAsync(fixture.FormId, payload, fixture.UserId));
        Assert.Contains("at most 30", ex.Message);
        Assert.Contains("valid email", ex.Message);
        Assert.Contains("configured options", ex.Message);
    }

    [Fact]
    public async Task SubmitAsync_ToInactiveForm_ThrowsInvalidOperationException()
    {
        var fixture = await SeedAsync(formActive: false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.SubmitAsync(fixture.FormId, ValidPayload(), fixture.UserId));
    }

    [Fact]
    public async Task SubmitAsync_ToNonexistentForm_ThrowsKeyNotFoundException()
    {
        var fixture = await SeedAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => fixture.Service.SubmitAsync(9999, ValidPayload(), fixture.UserId));
    }

    [Fact]
    public async Task SubmitAsync_OptionalCheckboxOmitted_StillSucceeds()
    {
        var fixture = await SeedAsync();
        var payload = new SubmitFormRequestDto
        {
            Values = Values(new { employeeName = "Jane Doe", daysRequested = 5, email = "jane@example.com", leaveType = "Sick" })
        };

        var result = await fixture.Service.SubmitAsync(fixture.FormId, payload, fixture.UserId);

        Assert.True(result.Id > 0);
    }
}
