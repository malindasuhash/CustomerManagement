using ApiContract;

namespace Api.Controllers
{
    internal class ApiContractProductAgreement_ToModelProductAgreementMap
    {
        internal static StateManagment.Entity.ProductAgreement Convert(CreateUpdateProductAgreement productAgreement, string legalEntityId)
        {
            return new StateManagment.Entity.ProductAgreement()
            {
                RateCardId = productAgreement.Rate_card_id,
                LegalEntityId = legalEntityId,
                DisplayName = productAgreement.Display_name,
            };
        }

    }
}