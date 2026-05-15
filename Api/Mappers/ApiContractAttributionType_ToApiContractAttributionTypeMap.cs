using StateManagment.Entity;

namespace Api.Mappers
{
    internal class ApiContractAttributionType_ToApiContractAttributionTypeMap
    {
        internal static Attribution[] Convert(ICollection<ApiContract.AttributionType> attributions)
        {
            if (attributions == null)
            {
                return null;
            }

            var attributionTypeList = new List<Attribution>();

            foreach ( var attribution in attributions)
            {
                switch (attribution)
                {
                    case ApiContract.AttributionType.Introducer:
                        attributionTypeList.Add(Attribution.Introducer);
                        break;
                    case ApiContract.AttributionType.Affiliate:
                        attributionTypeList.Add(Attribution.Affiliate);
                        break;
                    case ApiContract.AttributionType.Reseller:
                        attributionTypeList.Add(Attribution.Reseller);
                        break;
                    case ApiContract.AttributionType.Other:
                        attributionTypeList.Add(Attribution.Other);
                        break;
                }
            }

            return attributionTypeList.ToArray();
        }
    }
}