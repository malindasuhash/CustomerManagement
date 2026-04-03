namespace StateManagment.Models
{
    public enum RuntimeStatus
    {
        None,
        INITIATE,
        EVALUATION_STARTED,
        EVALUATION_COMPLETED,
        EVALUATION_INCOMPLETE,
        EVALUATION_REQUIRES_MANUAL_REVIEW,
        EXTERNAL_CHANGES_DETECTED,
        CHANGE_FAILED,
        CHANGE_APPLIED
    }
}
