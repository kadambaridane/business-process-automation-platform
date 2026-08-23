using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.DTOs.Forms;

public class FormFieldDto
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? DefaultValue { get; set; }
    public FieldValidationRulesDto? ValidationRules { get; set; }
}
