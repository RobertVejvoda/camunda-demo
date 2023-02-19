using Camunda.Abstractions;
using Camunda.Command;
using Common;
using Dapr.Client;
using Microsoft.Extensions.Logging;

namespace Camunda
{
    public class ZeebeClient : IZeebeClient
    {
        private readonly DaprClient daprClient;
        private readonly ILogger logger;

        public ZeebeClient(DaprClient daprClient, ILogger<ZeebeClient> logger)
        {
            this.daprClient = daprClient;
            this.logger = logger;
        }

        public async Task<CreateInstanceResponse> CreateInstanceAsync(CreateInstanceRequest request)
        {
            if (request.BpmnProcessId == null && request.ProcessDefinitionKey == null)
            {
                throw new ArgumentException("Set either bpmnProcessId or processDefinitionKey.");
            }

            return await daprClient.InvokeBindingAsync<CreateInstanceRequest, CreateInstanceResponse>(Bindings.ZeebeCommand, Commands.CreateInstance, request);
        }

        public Task CancelInstanceAsync(CancelInstanceRequest request)
            => daprClient.InvokeBindingAsync(Bindings.ZeebeCommand, Commands.CancelInstance, request);

        public Task<SetVariablesResponse> SetVariablesAsync(SetVariablesRequest request)
            => daprClient.InvokeBindingAsync<SetVariablesRequest, SetVariablesResponse>(Bindings.ZeebeCommand, Commands.SetVariables, request);

        public Task ResolveIncidentAsync(ResolveIncident request)
            => daprClient.InvokeBindingAsync(Bindings.ZeebeCommand, Commands.ResolveIncident, request);

        public Task<PublishMessageResponse> PublishMessageAsync(PublishMessageRequest request)
            => daprClient.InvokeBindingAsync<PublishMessageRequest, PublishMessageResponse>(Bindings.ZeebeCommand, Commands.PublishMessage, request);

        public Task<IList<ActivatedJob>> ActivateJobsAsync(ActivateJobsRequest request)
            => daprClient.InvokeBindingAsync<ActivateJobsRequest, IList<ActivatedJob>>(Bindings.ZeebeCommand, Commands.ActivateJobs, request);
        
        public Task CompleteJobAsync(CompleteJobRequest request)
            => daprClient.InvokeBindingAsync(Bindings.ZeebeCommand, Commands.CompleteJob, request);

        public Task FailJobAsync(FailJobRequest request)
            => daprClient.InvokeBindingAsync(Bindings.ZeebeCommand, Commands.FailJob, request);

        public Task UpdateJobRetriesAsync(UpdateJobRetriesRequest request)
            => daprClient.InvokeBindingAsync(Bindings.ZeebeCommand, Commands.UpdateJobRetries, request);

        public Task ThrowErrorAsync(ThrowErrorRequest request)
            => daprClient.InvokeBindingAsync(Bindings.ZeebeCommand, Commands.ThrowError, request);

        public Task<TopologyResponse> GetTopologyAsync()
            => daprClient.InvokeBindingAsync<object, TopologyResponse>(Bindings.ZeebeCommand, Commands.Topology, new { });

    }
}