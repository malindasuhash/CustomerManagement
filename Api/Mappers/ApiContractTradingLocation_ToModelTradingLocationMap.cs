namespace Api.Mappers
{
    internal class ApiContractTradingLocation_ToModelTradingLocationMap
    {
        internal static StateManagment.Entity.TradingLocation Convert(ApiContract.CreateTradingLocation tradingLocation, string legalEntityId)
        {
            var tradingAt = new StateManagment.Entity.TradingLocation()
            {
                LegalEntityId = legalEntityId,
                Website = tradingLocation.Website,
                Name = tradingLocation.Name,
                Address = ApiContractAddress_ToModelAddressMap.Convert(tradingLocation.Address),
                Contacts = ApiContractTradingLocationContact_ToModelContactReferencesMap.Convert(tradingLocation.Contacts)
            };

            if (tradingLocation.Labels != null)
            {
                tradingAt.Labels = [.. tradingLocation.Labels];
            }
            if (tradingLocation.Meta_data != null)
            {
                tradingAt.MetaData = tradingLocation.Meta_data.Select(md => new StateManagment.Entity.MetaDataModel() { Key = md.Key, Value = md.Value }).ToArray();
            }
            if (tradingLocation.System_data != null)
            {
                tradingAt.SystemData = tradingLocation.System_data.Select(sd => new StateManagment.Entity.SystemDataModel() { Key = sd.Key, Value = sd.Value }).ToArray();
            }

            return tradingAt;
        }

        internal static StateManagment.Entity.TradingLocation Update(ApiContract.UpdateTradingLocation tradingLocation, string legalEntityId)
        {
            var tradingAt = new StateManagment.Entity.TradingLocation()
            {
                LegalEntityId = legalEntityId,
                Website = tradingLocation.Website,
                Name = tradingLocation.Name,
                Address = ApiContractAddress_ToModelAddressMap.Convert(tradingLocation.Address),
                Contacts = ApiContractTradingLocationContact_ToModelContactReferencesMap.Convert(tradingLocation.Contacts)
            };

            if (tradingLocation.Labels != null)
            {
                tradingAt.Labels = [.. tradingLocation.Labels];
            }
            if (tradingLocation.Meta_data != null)
            {
                tradingAt.MetaData = tradingLocation.Meta_data.Select(md => new StateManagment.Entity.MetaDataModel() { Key = md.Key, Value = md.Value }).ToArray();
            }
            if (tradingLocation.System_data != null)
            {
                tradingAt.SystemData = tradingLocation.System_data.Select(sd => new StateManagment.Entity.SystemDataModel() { Key = sd.Key, Value = sd.Value }).ToArray();
            }

            return tradingAt;
        }
    }
}