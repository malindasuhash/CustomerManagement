using ExternalAdapter.Interfaces;
using StateManagment.Entity;
using StateManagment.Models;

namespace ExternalAdapter.Infrastructure
{
    public class QueryApi : IQueryApi
    {
        public IEnumerable<MessageEnvelop> GetLegalEntitiesByContactId(string customerId, string contactId)
        {
            var messageEnvelop = new MessageEnvelop()
            {
                CustomerId = customerId,
                Applied = new LegalEntity()
                {
                    BusinessContacts =
                    [
                        new() { ContactType = ContactType.Account, ContactId = contactId }
                    ]
                },
                Submitted = new LegalEntity()
                {
                    BusinessContacts =
                    [
                        new() { ContactType = ContactType.Account, ContactId = contactId }
                    ]
                }
            };

            return [messageEnvelop];
        }

        public IEnumerable<MessageEnvelop> GetTradingLocationsByContactId(string customerId, string legalEntityId, string tradingLocationId)
        {
            var messageEnvelop = new MessageEnvelop()
            {
                CustomerId = customerId,
                EntityId = tradingLocationId,
                Applied = new TradingLocation()
                {
                    Contacts =
                    [
                        new() { ContactType = ContactType.Account, ContactId = legalEntityId }
                    ]
                },
                Submitted = new TradingLocation()
                {
                    Contacts =
                    [
                        new() { ContactType = ContactType.Account, ContactId = legalEntityId }
                    ]
                }
            };
            
            return [messageEnvelop];
        }
    }
}
