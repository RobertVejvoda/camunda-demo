namespace cms;

public record ClientPortfolio(
    string SubscriptionId, 
    string ClientId,
    decimal PortfolioValue,
    decimal ClaimsValue);