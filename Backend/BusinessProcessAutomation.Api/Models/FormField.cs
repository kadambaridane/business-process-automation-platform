namespace BusinessProcessAutomation.Api.Models;

public class FormField
{
    public int Id { get; set; }

    public int FormDefinitionId { get; set; }
    public FormDefinition FormDefinition { get; set; } = null!;

    public string FieldName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? DefaultValue { get; set; }

    /// <summary>JSON-encoded validation rules (e.g. min/max, regex) and, for Dropdown/Radio, the option list.</summary>
    public string? ValidationRulesJson { get; set; }
}
