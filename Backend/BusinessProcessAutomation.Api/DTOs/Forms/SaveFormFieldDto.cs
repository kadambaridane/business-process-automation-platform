using System.ComponentModel.DataAnnotations;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.DTOs.Forms;

public class SaveFormFieldDto
{
    [Required, MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Label { get; set; } = string.Empty;

    public FieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int? DisplayOrder { get; set; }
    public string? DefaultValue { get; set; }
    public FieldValidationRulesDto? ValidationRules { get; set; }
}
