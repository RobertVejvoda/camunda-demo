namespace muw;

public record SubscriptionStateHistory(string SubscriptionId, SubscriptionState State, string Message, DateTime ChangedOn);
