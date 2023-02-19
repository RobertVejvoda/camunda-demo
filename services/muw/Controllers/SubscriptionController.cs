namespace muw.Controllers;

[ApiController]
[Route("[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly ILogger<SubscriptionController> logger;
    private readonly DaprClient daprClient;

    public SubscriptionController(ILogger<SubscriptionController> logger, DaprClient daprClient)
    {
        this.logger = logger;
        this.daprClient = daprClient;
    }

    // This method gets called from Camunda workflow.
    [HttpPost("/validation-requested")]
    public async Task<IActionResult> SubscriptionValidationRequested(SubscriptionValidationRequest request)
    {
        // validate incoming message
        if (!ValidateSubscriptionRequest(request)) return BadRequest();
        
        // store subscription in a state store
        var state = await daprClient.GetStateEntryAsync<Subscription>(Bindings.StateStore, Key(request.SubscriptionId));
        state.Value ??= Subscription.Create(request);
        await state.SaveAsync();

        // notify state changed
        await daprClient.PublishEventAsync(Bindings.PubSub, Topics.SubscriptionStateChanged,
            state.Value.CurrentState);

        var zeebeProcessInstanceKey = Request.Headers[Headers.ZeebeProcessInstanceKey]; 
        logger.LogInformation("Subscription {SubscriptionId} created. Zeebe ProcessInstanceKey: {ZeebeProcessInstanceKey}", request.SubscriptionId, zeebeProcessInstanceKey);
        
        return Ok();
    }
    
    // This method gets called from Camunda workflow.
    [HttpPost("/register")]
    public async Task<IActionResult> RegisterSubscription(SubscriptionRecord record)
    {
        // validation incoming message
        if (!ValidateSubscriptionRecord(record)) return BadRequest();
        
        // store subscription in a state store
        var state = await daprClient.GetStateEntryAsync<Subscription>(Bindings.StateStore, Key(record.SubscriptionId));
        if (state.Value is null) return NotFound();
        
        var subscription = state.Value;
        subscription.Registered(DateTime.Now);
        await state.SaveAsync();
        
        logger.LogInformation("Subscription {SubscriptionId} registered", record.SubscriptionId);

        // notify state changed
        await daprClient.PublishEventAsync(Bindings.PubSub, Topics.SubscriptionStateChanged,
            subscription.CurrentState);
        
        return Ok();
    }

    // This method gets called from Camunda workflow.
    [HttpPost("/validate")]
    public async Task<IActionResult> ValidateSubscription(SubscriptionRecord record)
    {
        // validation incoming message
        if (!ValidateSubscriptionRecord(record)) return BadRequest();
        
        var state = await daprClient.GetStateEntryAsync<Subscription>(Bindings.StateStore, Key(record.SubscriptionId));
        if (state.Value is null) return NotFound();

        await daprClient.PublishEventAsync(Bindings.PubSub, Topics.SubscriptionValidationRequested,
            record);
        
        logger.LogInformation("Subscription {SubscriptionId} validation requested", record.SubscriptionId);
        
        return Ok();
    }

    [HttpPost("/validated")]
    public async Task<IActionResult> SubscriptionValidated(SubscriptionRecord record, [FromServices] IZeebeClient zeebeClient)
    {
        // validation incoming message
        if (!ValidateSubscriptionRecord(record)) return BadRequest();
        
        var state = await daprClient.GetStateEntryAsync<Subscription>(Bindings.StateStore, Key(record.SubscriptionId));
        if (state.Value is null) return NotFound();

        var subscription = state.Value;
        subscription.Validated(DateTime.Now, record.PortfolioValue, record.ClaimsValue);
        await state.SaveAsync();
        
        logger.LogInformation("Subscription {SubscriptionId} validated", record.SubscriptionId);
        
        return Ok();
    }
  
    [HttpPost("/notify-assessor")]
    public async Task<IActionResult> NotifyAssessor(SubscriptionRecord record)
    {
        // validation incoming message
        if (!ValidateSubscriptionRecord(record)) return BadRequest();
        
        var state = await daprClient.GetStateEntryAsync<Subscription>(Bindings.StateStore, Key(record.SubscriptionId));
        if (state.Value is null) return NotFound();
        
        var body = ComposeNotificationMessageToAssessor(state.Value);
        var metadata = new Dictionary<string, string>
        {
            ["emailFrom"] = "noreply@incredible.inc",
            ["emailTo"] = "muw-assessors@incredible.inc",
            ["subject"] = $"Manual assessment needed for Subscription: {record.SubscriptionId}"
        };
        
        logger.LogInformation("Subscription {SubscriptionId} - assessor notified", record.SubscriptionId);

        await daprClient.InvokeBindingAsync(Bindings.Smtp, "create", body, metadata);

        return Ok();
    }

    [HttpPost("/accepted")]
    public async Task<IActionResult> SubscriptionAccepted(SubscriptionRecord record)
    {
        // validation incoming message
        if (!ValidateSubscriptionRecord(record)) return BadRequest();
        
        var state = await daprClient.GetStateEntryAsync<Subscription>(Bindings.StateStore, Key(record.SubscriptionId));
        if (state.Value is null) return NotFound();

        state.Value.Accepted(DateTime.Now);
        await state.SaveAsync();

        logger.LogInformation("Subscription {SubscriptionId} accepted", record.SubscriptionId);
        
        await daprClient.PublishEventAsync(Bindings.StateStore, Topics.SubscriptionStateChanged,
            record);
        
        return Ok();
    }

    [HttpPost("/rejected")]
    public async Task<IActionResult> SubscriptionRejected(SubscriptionRecord record)
    {
        // validation incoming message
        if (!ValidateSubscriptionRecord(record)) return BadRequest();
        
        var state = await daprClient.GetStateEntryAsync<Subscription>(Bindings.StateStore, Key(record.SubscriptionId));
        if (state.Value is null) return NotFound();

        var subscription = state.Value;
        subscription.Rejected(DateTime.Now, record.RejectReason);
        
        await state.SaveAsync();
        
        logger.LogInformation("Subscription {SubscriptionId} rejected", record.SubscriptionId);
        
        await daprClient.PublishEventAsync(Bindings.StateStore, Topics.SubscriptionStateChanged, subscription.CurrentState);

        return Ok();
    }
    
    [HttpPost("/cancellation-requested")]
    public async Task<IActionResult> SubscriptionCancellationRequested(SubscriptionCancellationRequest request)
    {
        logger.LogInformation("Subscription {SubscriptionId} cancellation requested", request.SubscriptionId);
        
        var state = await daprClient.GetStateEntryAsync<Subscription>(Bindings.StateStore, Key(request.SubscriptionId));
        if (state.Value is null) return NotFound();

        var subscription = state.Value;
        subscription.Cancelled(DateTime.Now);
        
        await state.SaveAsync();
        
        logger.LogInformation("Subscription {SubscriptionId} cancelled", request.SubscriptionId);
        
        await daprClient.PublishEventAsync(Bindings.StateStore, Topics.SubscriptionStateChanged, subscription.CurrentState);

        return Ok();
    }
    
    // subscribe to RabbitMQ topic
    [Topic(Bindings.PubSub, Topics.ClientPortfolioChecked)]
    [HttpPost(Topics.ClientPortfolioChecked)]
    public async Task<IActionResult> OnClientPortfolioChecked(ClientPortfolio portfolio, [FromServices] IZeebeClient zeebeClient)
    {
        // validation incoming message
        if (!ValidateClientPortfolio(portfolio)) return BadRequest();
         
        logger.LogInformation("Subscription {SubscriptionId} - client portfolio checked", portfolio.SubscriptionId);
        
        // send message to Camunda to continue the process
        await zeebeClient.PublishMessageAsync(new PublishMessageRequest("validated", portfolio.SubscriptionId, null,
            "30s", new Dictionary<string, string>()
            {
                ["portfolioValue"] = portfolio.PortfolioValue.ToString(CultureInfo.InvariantCulture),
                ["claimsValue"] = portfolio.ClaimsValue.ToString(CultureInfo.InvariantCulture)
            }));

        return Ok();
    }
    
    private static bool ValidateSubscriptionRequest(SubscriptionValidationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SubscriptionId)) return false;
        if (request.InterestRate < 0) return false;
        if (request.LoanAmount > request.MortgageAmount) return false;
        if (string.IsNullOrWhiteSpace(request.MessageId)) return false;
        if (string.IsNullOrWhiteSpace(request.ClientId)) return false;

        return true;
    }
    
    private static bool ValidateClientPortfolio(ClientPortfolio record)
    {
        if (string.IsNullOrWhiteSpace(record.SubscriptionId)) return false;
        if (string.IsNullOrWhiteSpace(record.ClientId)) return false;
        if (record.PortfolioValue < 0) return false;
        if (record.ClaimsValue < 0) return false;
        
        return true;
    }
    
    private static bool ValidateSubscriptionRecord(SubscriptionRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.SubscriptionId)) return false;
        if (string.IsNullOrWhiteSpace(record.ClientId)) return false;
        return true;
    }

    private static string Key(string subscriptionId) => $"subscription-{subscriptionId}";

    private static string ComposeNotificationMessageToAssessor(Subscription subscription)
    {
        var sb = new StringBuilder();
        sb.AppendLine("New subscription validation request has arrived:");
        sb.AppendLine();
        sb.AppendLine($"Mortgage: {subscription.Request.MortgageAmount}");
        sb.AppendLine($"Interest Rate: {subscription.Request.InterestRate}");
        sb.AppendLine($"Loan: {subscription.Request.LoanAmount}");
        sb.AppendLine($"Client Portfolio: {subscription.PortfolioValue}");
        sb.AppendLine($"Client Claims: {subscription.ClaimsValue}");
        sb.AppendLine();
        foreach (var s in subscription.StateHistory)
            sb.AppendLine(s.Message);

        return sb.ToString();
    } 

}