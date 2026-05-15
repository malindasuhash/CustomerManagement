using ApiContract;
using StateManagment.Entity;

namespace Api.Mappers
{
    internal class ApiContractAttributionTypes_ToModelAttributionTypeMap
    {
        internal static Attribution[] Convert(ICollection<AttributionType> attribution_type)
        {
            if (attribution_type == null)
            {
                return null;
            }

            var converted_attribution_types = new List<Attribution>();

            foreach (var attribution_type_item in attribution_type)
            {
                switch (attribution_type_item)
                {
                    case AttributionType.Introducer:
                        converted_attribution_types.Add(Attribution.Introducer);
                        break;
                    case AttributionType.Affiliate:
                        converted_attribution_types.Add(Attribution.Affiliate);
                        break;
                    case AttributionType.Reseller:
                        converted_attribution_types.Add(Attribution.Reseller);
                        break;
                    case AttributionType.Other:
                        converted_attribution_types.Add(Attribution.Other);
                        break;
                    default:
                        converted_attribution_types.Add(Attribution.None);
                        break;
                }
            }

            return converted_attribution_types.ToArray();
        }
    }
}