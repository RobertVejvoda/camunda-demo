namespace Camunda.Command
{
    public record CreateInstanceResponse(
        long? ProcessDefinitionKey,
        string BpmnProcessId,
        int? Version,
        long? ProcessInstanceKey);
}