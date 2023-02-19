namespace sim.Controllers;

[ApiController]
[Route("[controller]")]
public class MuwController : ControllerBase
{
    private readonly ILogger<MuwController> logger;
    private readonly IZeebeClient zeebeClient;

    public MuwController(ILogger<MuwController> logger, IZeebeClient zeebeClient)
    {
        this.logger = logger;
        this.zeebeClient = zeebeClient;
    }

    [HttpPost("/cron")]
    public async Task<IActionResult> TriggerSubscriptionValidationRequest()
    {
        var request = RandomSubscriptionValidationRequest();
        
        // start Camunda process
        // var createInstanceRequest = new CreateInstanceRequest("subscription-validation", null, null, request.ToDictionary());
        // await zeebeClient.CreateInstanceAsync(createInstanceRequest);

        // start Camunda process by invoking a message
        var publishMessageRequest = new PublishMessageRequest("validation-requested", request.SubscriptionId, null,
            "30s", request.ToDictionary());
        await zeebeClient.PublishMessageAsync(publishMessageRequest);
        
        logger.LogInformation("New Camunda process started for {SubscriptionId}", request.SubscriptionId);
        
        return Ok();
    }

    [HttpPost("/cancel")]
    public async Task<IActionResult> CancelSubscriptionRequest(SubscriptionCancellationRequest request)
    {
        await zeebeClient.CancelInstanceAsync(new CancelInstanceRequest(request.ZeebeProcessInstanceKey));

        return Accepted();
    }

    [Topic(Bindings.PubSub, Topics.SubscriptionStateChanged)]
    [HttpPost(Topics.SubscriptionStateChanged)]
    public IActionResult LogEvent(SubscriptionStateHistory stateHistory)
    {
        logger.LogInformation(stateHistory.Message);
        return Ok();
    }

    private static SubscriptionValidationRequest RandomSubscriptionValidationRequest()
    {
        // create and trigger request
        var subscriptionId = Guid.NewGuid().ToString();
        var milliseconds = DateTime.Now.Millisecond;
        var year = Random.Shared.Next(1970, DateTime.Now.Year - 17);
        var month = Random.Shared.Next(1, 63);
        var day = Random.Shared.Next(1, 29);
        var ending = DateTime.Now.Millisecond.ToString("0000");
        var clientId = $"{year}{month}{day}/{ending}";
        var messageId = $"subscription-{milliseconds}";
        var mortgageAmount = Random.Shared.NextInt64(100_000, 10_000_000);
        var interestRate = Math.Round(Random.Shared.NextDouble() * 10, 2);
        var loanAmount = Random.Shared.NextInt64(100_000, mortgageAmount);
        var request = new SubscriptionValidationRequest(
            messageId,
            subscriptionId,
            clientId,
            mortgageAmount,
            interestRate,
            loanAmount);
        return request;
    }

    

}