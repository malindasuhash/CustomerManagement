using System;

namespace StateManagment.Models
{
    // Simple immutable correlation context
    public sealed record CorrelationContext(Guid CorrelationId, string ClientId);
}
