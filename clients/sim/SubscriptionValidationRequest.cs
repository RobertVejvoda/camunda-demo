namespace sim;

public record SubscriptionValidationRequest(string MessageId, string SubscriptionId, string ClientId,
    decimal MortgageAmount, double InterestRate, decimal LoanAmount)
{
    public Dictionary<string, string> ToDictionary()
    {
        var jsonSerializerSettings = new JsonSerializerSettings 
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        
        var json = JsonConvert.SerializeObject(this, jsonSerializerSettings);
        var variables = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        return variables!;
    }
}