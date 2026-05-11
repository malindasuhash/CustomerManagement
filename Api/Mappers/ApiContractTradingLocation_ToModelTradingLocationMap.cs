using ApiContract;

namespace Api.Mappers
{
    internal class ApiContractTradingLocation_ToModelTradingLocationMap
    {
        internal static StateManagment.Entity.TradingLocation Convert(CreateUpdateTradingLocation tradingLocation, string legalEntityId)
        {
            return new StateManagment.Entity.TradingLocation()
            {
                LegalEntityId = legalEntityId,
                Website = tradingLocation.Website,
                Name = tradingLocation.Name,
            };
        }
    }
}