using StateManagment.Models;

namespace ContactOrchestration.Checks
{
    internal class EvalutionComplete : Check
    {
        public EvalutionComplete() : base(null)
        {
            
        }

        public override Task RunCheckAsync(OrchestrationInfo runtimeInfo)
        {
            //NOP - This is the end of the chain, there is nothing to do here.
            return Task.CompletedTask; 
        }
    }
}
