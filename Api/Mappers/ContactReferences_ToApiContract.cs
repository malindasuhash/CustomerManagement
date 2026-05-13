using StateManagment.Entity;

namespace Api.Mappers
{
    public class ContactReferences_ToApiContract
    {
        public static ApiContract.TradingLocationContact[] Convert(ContactReference[] contactReferences)
        {
            if (contactReferences == null)
            {
                return null;
            }
            var responseContacts = new List<ApiContract.TradingLocationContact>();
            foreach (var contact in contactReferences)
            {
                responseContacts.Add(new ApiContract.TradingLocationContact()
                {
                    Contact_id = contact.ContactId,
                    Contact_type = ContactType_ToTradingLocationContactTypeApiContractMap.Convert(contact.ContactType)
                });
            }
            return responseContacts.ToArray();
        }
    }
}