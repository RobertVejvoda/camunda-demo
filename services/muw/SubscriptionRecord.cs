namespace muw;

public record SubscriptionRecord(
    string SubscriptionId, 
    string ClientId, 
    decimal PortfolioValue,
    decimal ClaimsValue,
    bool IsEligible,
    bool IsAccepted,
    bool IsRejected,
    string RejectReason);