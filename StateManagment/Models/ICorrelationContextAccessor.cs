namespace StateManagment.Models
{
    public interface ICorrelationContextAccessor
    {
        CorrelationContext? Context { get; set; }
    }
}
