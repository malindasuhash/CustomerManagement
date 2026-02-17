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
        IN_PROGRESS,
        SYNCHONISED
    }
}
