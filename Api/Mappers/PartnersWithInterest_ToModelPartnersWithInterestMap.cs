using StateManagment.Entity;

namespace Api.Mappers
{
    internal class PartnersWithInterest_ToModelPartnersWithInterestMap
    {
        internal static PartnersWithInterest[] Convert(ICollection<ApiContract.PartnerWithInterest> partners_with_interest)
        {
            if (partners_with_interest == null)
            {
                return null;
            }

            var modelPartnersWithInterestList = new List<PartnersWithInterest>();

            foreach (var partner in partners_with_interest)
            {
                var modelPartnerWithInterest = new PartnersWithInterest()
                {
                    LegalEntityId = partner.Legal_entity_id,
                    Attributions = ApiContractAttributionType_ToApiContractAttributionTypeMap.Convert(partner.Attribution_type),
                };

                if (partner.Meta_data != null)
                {
                    var metaDataList = new List<MetaDataModel>();
                    foreach (var data in partner.Meta_data)
                    {
                        metaDataList.Add(new MetaDataModel() { Key = data.Key, Value = data.Value });
                    }

                    modelPartnerWithInterest.MetaData = metaDataList.ToArray();
                }

                modelPartnersWithInterestList.Add(modelPartnerWithInterest);
            }

            return modelPartnersWithInterestList.ToArray();
        }
    }
}