namespace StateManagment.Models
{
    public enum FeedbackType
    {
        None,
        DocumentRequired,
        WaitingForExternalRiskChecks,
        LegalEntityMissing,
        WaitingForProductSelection,
        MissingRequiredInformation,
        InternalError,
        WaitingForContractSignatureOrAcceptance,
        UserActionRequired,
        WaitingForLegalEntityApproval,
        WaitingForConfiguration
    }
}
