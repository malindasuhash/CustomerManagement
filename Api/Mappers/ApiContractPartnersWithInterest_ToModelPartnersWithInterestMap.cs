using StateManagment.Entity;

namespace Api.Mappers
{
    internal class ApiContractPartnersWithInterest_ToModelPartnersWithInterestMap
    {
        internal static PartnersWithInterest[] Convert(ICollection<ApiContract.PartnerWithInterest> partners_with_interest)
        {
            if (partners_with_interest == null)
            {
                return Array.Empty<PartnersWithInterest>();
            }

            var converted_partners_with_interest = new List<PartnersWithInterest>();
            foreach (var partner_with_interest in partners_with_interest)
            {
                //var partner_with_interest_model = new PartnerWithInterest()
                //{
                //    Legal_entity_id = partner_with_interest.Legal_entity_id,
                //    Attribution_type = partner_with_interest.Attribution_type,
                //};

            }

            return converted_partners_with_interest.ToArray();
        }
    }
}