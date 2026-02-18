namespace StateManager.Models
{
    public enum EntityName
    {
        None,
        Contact,
        BankAccount,
    }

    public enum EntityState
    {
        None,
        NEW,
        EVALUATING,
        EVALUATION_RESTARTING,
        IN_PROGRESS,
        SYNCHONISED,
    }
}
