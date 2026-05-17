using System.Threading;

namespace StateManagment.Models
{
    // Singleton that uses AsyncLocal to store per-execution correlation context
    public class CorrelationContextAccessor : ICorrelationContextAccessor
    {
        private static readonly AsyncLocal<CorrelationContext?> _current = new();

        public CorrelationContext? Context
        {
            get => _current.Value;
            set => _current.Value = value;
        }
    }
}
