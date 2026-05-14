using ApiContract;
using StateManagment.Entity;

namespace Api.Mappers
{
    internal class ApiContractProductAgreement_ToModelProductAgreementMap
    {
        internal static StateManagment.Entity.ProductAgreement Convert(CreateProductAgreement productAgreement, string legalEntityId)
        {
            var agreement = new StateManagment.Entity.ProductAgreement()
            {
                RateCardId = productAgreement.Rate_card_id,
                LegalEntityId = legalEntityId,
                DisplayName = productAgreement.Display_name,
                BillingGroupId = productAgreement.Billing_group_id,
                ProductCatalogueId = productAgreement.Product_catalogue_id,
                TradingLocationId = productAgreement.Trading_location_id,
            };

            if (productAgreement.Labels != null)
            {
                agreement.Labels = [.. productAgreement.Labels];
            }
            if (productAgreement.Meta_data != null)
            {
                var metaDataList = new List<MetaDataModel>();
                foreach (var data in productAgreement.Meta_data)
                {
                    metaDataList.Add(new MetaDataModel { Key = data.Key, Value = data.Value });
                }

                agreement.MetaData = [.. metaDataList];
            }
            if (productAgreement.System_data != null)
            {
                var systemDataList = new List<SystemDataModel>();
                foreach (var data in productAgreement.System_data)
                {
                    systemDataList.Add(new SystemDataModel { Key = data.Key, Value = data.Value });
                }

                agreement.SystemData = [.. systemDataList];
            }

            return agreement;
        }

        internal static StateManagment.Entity.ProductAgreement Update(UpdateProductAgreement productAgreement, string legalEntityId)
        {
            var agreement = new StateManagment.Entity.ProductAgreement()
            {
                RateCardId = productAgreement.Rate_card_id,
                LegalEntityId = legalEntityId,
                DisplayName = productAgreement.Display_name,
                BillingGroupId = productAgreement.Billing_group_id,
                ProductCatalogueId = productAgreement.Product_catalogue_id,
                TradingLocationId = productAgreement.Trading_location_id,
            };

            if (productAgreement.Labels != null)
            {
                agreement.Labels = [.. productAgreement.Labels];
            }
            if (productAgreement.Meta_data != null)
            {
                var metaDataList = new List<MetaDataModel>();
                foreach (var data in productAgreement.Meta_data)
                {
                    metaDataList.Add(new MetaDataModel { Key = data.Key, Value = data.Value });
                }

                agreement.MetaData = [.. metaDataList];
            }
            if (productAgreement.System_data != null)
            {
                var systemDataList = new List<SystemDataModel>();
                foreach (var data in productAgreement.System_data)
                {
                    systemDataList.Add(new SystemDataModel { Key = data.Key, Value = data.Value });
                }

                agreement.SystemData = [.. systemDataList];
            }

            return agreement;
        }
    }
}