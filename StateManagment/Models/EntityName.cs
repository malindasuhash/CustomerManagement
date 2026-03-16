namespace StateManagment.Models
{
    public enum EntityName
    {
        None,
        Contact,
        BankAccount,
        LegalEntity,
        BillingGroup,
        ProductAgreement
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
