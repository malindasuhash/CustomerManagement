using Api.ApiModels;

namespace Api.Mappers
{
    internal class TradingLocation_ToApiContractMap
    {
        public static ApiContract.TradingLocation Convert(dynamic tradingLocation)
        {
            if (tradingLocation == null)
            {
                return null;
            }
            var responseTradingLocation = new ApiContract.TradingLocation()
            {
                Name = tradingLocation.Name,
                Address = Address_ToApiContractMap.Convert(tradingLocation.Address),
                Legal_entity_id = tradingLocation.LegalEntityId,
                Contacts = ContactReferences_ToApiContract.Convert(tradingLocation.Contacts),
            };
            if (tradingLocation.Labels != null)
            {
                var labels = new ApiContract.Labels();
                foreach (var label in tradingLocation.Labels)
                {
                    labels.Add(label);
                }
                responseTradingLocation.Labels = labels;
            }
            if (tradingLocation.MetaData != null)
            {
                var metaData = new ApiContract.MetaData();
                foreach (var data in tradingLocation.MetaData)
                {
                    metaData.Add(data.Key, data.Value);
                }
                responseTradingLocation.Meta_data = metaData;
            }
            if (tradingLocation.SystemData != null)
            {
                var systemData = new ApiContract.SystemData();
                foreach (var data in tradingLocation.SystemData)
                {
                    systemData.Add(data.Key, data.Value);
                }
                responseTradingLocation.System_data = systemData;
            }
            return responseTradingLocation;
        }
    }
}