using System.Runtime.CompilerServices;

namespace muw;

public enum SubscriptionState
{
    New = 1,
    Registered = 2,
    Validated = 4,
    Accepted = 8,
    Rejected = 16,
    Cancelled = 32
}