
using StateManagment.Entity;

namespace Api.Mappers
{
    internal class ProductAgreement_ToApiContractMap
    {
        internal static ApiContract.ProductAgreement Convert(ProductAgreement productAgreement)
        {
            if (productAgreement == null) { return null; }

            var contract = new ApiContract.ProductAgreement()
            {
                Billing_group_id = productAgreement.BillingGroupId,
                // Legal_entity_id = productAgreement.LegalEntityId,
                Rate_card_id = productAgreement.RateCardId,
                Trading_location_id = productAgreement.TradingLocationId,
                Display_name = productAgreement.DisplayName,
                Product_id = productAgreement.ProductId
            };

            if (productAgreement.Labels != null)
            {
                var labels = new ApiContract.Labels();
                foreach (var label in productAgreement.Labels)
                {
                    labels.Add(label);
                }
                contract.Labels = labels;
            }

            if (productAgreement.MetaData != null)
            {
                var metaData = new ApiContract.MetaData();
                foreach (var data in productAgreement.MetaData)
                {
                    metaData.Add(data.Key, data.Value);
                }
                contract.Meta_data = metaData;
            }

            if (productAgreement.SystemData != null)
            {
                var systemData = new ApiContract.SystemData();
                foreach (var data in productAgreement.SystemData)
                {
                    systemData.Add(data.Key, data.Value);
                }
                contract.System_data = systemData;
            }

            return contract;
        }
    }
}