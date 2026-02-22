namespace ContactOrchestration.Checks
{
    internal class EvalutionComplete(Check nextCheck) : Check(nextCheck)
    {
        public override Task RunCheckAsync(RuntimeInfo runtimeInfo)
        {
            //NOP - This is the end of the chain, there is nothing to do here.
            return Task.CompletedTask; 
        }
    }
}
