namespace muw;

public record SubscriptionValidationRequest(
    string MessageId, 
    string SubscriptionId, 
    string ClientId, 
    decimal MortgageAmount, 
    double InterestRate, 
    decimal LoanAmount);