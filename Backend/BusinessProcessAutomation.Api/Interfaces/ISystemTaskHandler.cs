using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Interfaces;

/// <summary>
/// A pluggable automated action a System Task step can invoke. Register new implementations
/// in DI and the engine resolves them by <see cref="Key"/> (matched against WorkflowStep.SystemActionKey)
/// without any change to the engine itself.
/// </summary>
public interface ISystemTaskHandler
{
    string Key { get; }

    /// <returns>A short outcome string recorded on the step's execution history.</returns>
    Task<string> ExecuteAsync(WorkflowInstance instance, WorkflowStep step);
}
