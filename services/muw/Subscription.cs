namespace muw;

public class Subscription
{
    public SubscriptionValidationRequest Request { get; private set; }
    public List<SubscriptionStateHistory> StateHistory { get; private set; }
    public decimal PortfolioValue { get; private set; }
    public decimal ClaimsValue { get; private set; }
    public SubscriptionStateHistory CurrentState => StateHistory.Last();
    public string Id => Request.SubscriptionId;

    private Subscription(SubscriptionValidationRequest request)
    {
        Request = request;
        StateHistory = new List<SubscriptionStateHistory>();
    }
    
    public Subscription Registered(DateTime dateRegistered, string? message = default)
    {
        EnsureNotCancelled();

        if (CurrentState.State > SubscriptionState.Registered) return this;

        StateHistory.Add(new SubscriptionStateHistory(Id, SubscriptionState.Registered,
            $"Subscription registered on {dateRegistered:F}. {message ?? string.Empty}",
            dateRegistered));

        return this;
    }

    public Subscription Validated(DateTime dateValidated, decimal portfolioValue, decimal claimsValue)
    {
        EnsureNotCancelled();

        PortfolioValue = portfolioValue;
        ClaimsValue = claimsValue;
        
        var message = $"Total portfolio value: {portfolioValue}, total claims value: {claimsValue}";
        StateHistory.Add(new SubscriptionStateHistory(Id, SubscriptionState.Validated, $"Subscription validated on {dateValidated:F}. {message}",
            dateValidated));
        
        return this;
    }

    private void EnsureNotCancelled()
    {
        if (CurrentState.State == SubscriptionState.Cancelled)
            throw new Exception($"Subscription {Id} is already cancelled.");
    }

    public Subscription Accepted(DateTime dateAccepted, string? message = default)
    {
        EnsureNotCancelled();
   
        StateHistory.Add(new SubscriptionStateHistory(Id, SubscriptionState.Accepted, $"Subscription accepted on {dateAccepted:F}. {message ?? string.Empty}",
            dateAccepted));

        return this;
    }
    
    public Subscription Rejected(DateTime dateRejected, string rejectReason)
    {
        EnsureNotCancelled();
        
        if (rejectReason == null) throw new ArgumentNullException(nameof(rejectReason));
        StateHistory.Add(new SubscriptionStateHistory(Id, SubscriptionState.Rejected, $"Subscription rejected on {dateRejected:F}. Reason: {rejectReason}",
            dateRejected));

        return this;
    }

    public Subscription Cancelled(DateTime dateCancelled)
    {
        EnsureNotCancelled();
        
        StateHistory.Add(new SubscriptionStateHistory(Id, SubscriptionState.Cancelled, $"Subscription cancelled on {dateCancelled:F}.",
            dateCancelled));
        
        return this;
    }
    
    public static Subscription Create(SubscriptionValidationRequest request, ICollection<SubscriptionStateHistory>? stateHistory = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var subscription = new Subscription(request);
        if (stateHistory is null)
        {
            subscription.StateHistory = new List<SubscriptionStateHistory>();
            var now = DateTime.Now;
            subscription.StateHistory.Add(new SubscriptionStateHistory(subscription.Id, SubscriptionState.New, $"Subscription created on {now:F}", now));
        }
        else
        {
            subscription.StateHistory = stateHistory.ToList();
        }

        return subscription;
    }
}