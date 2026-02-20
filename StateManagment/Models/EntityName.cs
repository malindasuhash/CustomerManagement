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
        ATTENTION_REQUIRED,
        IN_REVIEW,
        IN_PROGRESS,
        SYNCHONISED
    }
}
