namespace Api.Mappers
{
    internal class ApiContractTradingLocationContact_ToModelContactReferencesMap
    {
        internal static StateManagment.Entity.ContactReference[] Convert(ICollection<ApiContract.TradingLocationContact> tradingLocationContacts)
        {
            if (tradingLocationContacts == null)
            {
                return null;
            }
            var contacts = new List<StateManagment.Entity.ContactReference>();
            foreach (var tradingLocationContact in tradingLocationContacts)
            {
                var contact = new StateManagment.Entity.ContactReference()
                {
                   ContactId = tradingLocationContact.Contact_id,
                   ContactType = ApiContractContactType_ToModelContactType.Convert(tradingLocationContact.Contact_type)
                };
                
                contacts.Add(contact);
            }

            return [.. contacts];
        }
    }
}