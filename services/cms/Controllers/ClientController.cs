namespace cms.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientController : ControllerBase
{
    private readonly ILogger<ClientController> logger;

    public ClientController(ILogger<ClientController> logger)
    {
        this.logger = logger;
    }

    [Topic(Bindings.PubSub, Topics.SubscriptionValidationRequested)]
    [HttpPost(Topics.SubscriptionValidationRequested)]
    public async Task<ActionResult<Guid>> CheckPortfolio(CheckClientPortfolioRequest request, [FromServices] DaprClient daprClient)
    {
        if (!ValidateRequest(request)) return BadRequest();
        
        logger.LogInformation("CheckPortfolio requested for {SubscriptionId}", request.SubscriptionId);
        
        // generate random client portfolio - would retrieve it from claims and policy management systems
        var result = GenerateRandomClientPortfolioResult(request);
        
        // simulate long running job from 5 to 10 seconds
        var sleepTime = Random.Shared.Next(5000, 10000);
        await Task.Delay(sleepTime);
        
        await daprClient.PublishEventAsync(Bindings.PubSub, Topics.ClientPortfolioChecked, result);
        
        return Ok();
    }

    private static ClientPortfolio GenerateRandomClientPortfolioResult(CheckClientPortfolioRequest request)
    {
        decimal portfolioValue = 0;
        decimal claimsValue = 0;
        var hasPortfolio = DateTime.Now.Ticks % 2 == 0;
        if (!hasPortfolio)
            return new ClientPortfolio(request.SubscriptionId, request.ClientId, portfolioValue, claimsValue);
       
        portfolioValue = Random.Shared.Next(100_000, 3_000_000);
        var hasClaims = Random.Shared.NextDouble() > 0.7;
        if (hasClaims)
        {
            claimsValue = Random.Shared.Next(100_000, 3_000_000); 
        }
        return new ClientPortfolio(request.SubscriptionId, request.ClientId, portfolioValue, claimsValue);
    }

    private static bool ValidateRequest(CheckClientPortfolioRequest request)
    {

        if (string.IsNullOrWhiteSpace(request.SubscriptionId)) return false;
        if (string.IsNullOrWhiteSpace(request.ClientId)) return false;

        return true;
    }
}
