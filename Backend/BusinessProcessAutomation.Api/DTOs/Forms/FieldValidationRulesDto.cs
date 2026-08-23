namespace BusinessProcessAutomation.Api.DTOs.Forms;

public class FieldValidationRulesDto
{
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }
    public string? Pattern { get; set; }

    /// <summary>Selectable options for Dropdown/Radio fields.</summary>
    public List<string>? Options { get; set; }
}
