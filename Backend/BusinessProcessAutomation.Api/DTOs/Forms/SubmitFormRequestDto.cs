using System.Text.Json;

namespace BusinessProcessAutomation.Api.DTOs.Forms;

public class SubmitFormRequestDto
{
    public Dictionary<string, JsonElement> Values { get; set; } = new();
}
