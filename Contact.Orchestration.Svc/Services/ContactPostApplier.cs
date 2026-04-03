using Contact.Orchestration.Svc.Contracts;
using Contact.Orchestration.Svc.Model;

namespace Contact.Orchestration.Svc.Services
{
    public class ContactPostApplier : IPostApplier
    {
        private readonly IApiQuery apiQuery;

        public ContactPostApplier(IApiQuery apiQuery)
        {
            this.apiQuery = apiQuery;
        }

        public async Task PostApply(RequestData requestData, CancellationToken stoppingToken)
        {
            // Post apply operation is to touch dependent entities.
            var impactedLegalEntities = await apiQuery.GetLegalEntitiesByContactId(requestData.EntityId);

            foreach (var legalEntity in impactedLegalEntities)
            {
                apiQuery.TouchLegalEntity(legalEntity.SearchBy());
            }

            // Trading locations where contact is used.
            var impactedTradingLocations = await apiQuery.GetTradingLocationsByContactId(requestData.CustomerId, requestData.EntityId);

            foreach (var tradingLocation in impactedTradingLocations)
            {
                apiQuery.TouchTradingLocation(tradingLocation.SearchBy());
            }
        }
    }
}
