namespace StateManagment.Models
{
    public enum EntityName
    {
        None,
        Contact,
        BankAccount,
        LegalEntity
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
        SYNCHRONISED
    }
}
