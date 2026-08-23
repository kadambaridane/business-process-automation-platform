namespace BusinessProcessAutomation.Api.Models;

public class FormDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public int ApplicationId { get; set; }
    public BusinessApplication Application { get; set; } = null!;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<FormField> FormFields { get; set; } = new List<FormField>();
    public ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
}
