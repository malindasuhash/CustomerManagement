
namespace Api.Mappers
{
    internal class BusinessContacts_ToModelBusinessContactsMap
    {
        internal static StateManagment.Entity.BusinessContact[] Convert(ICollection<ApiContract.BusinessContact> business_contacts)
        {
            if (business_contacts == null)
            {
                return null;
            }

            var modelBusinessContacts = new StateManagment.Entity.BusinessContact[business_contacts.Count];

            for (int i = 0; i < business_contacts.Count; i++)
            {
                var apiContractBusinessContact = business_contacts.ElementAt(i);
                modelBusinessContacts[i] = new StateManagment.Entity.BusinessContact()
                {
                    ContactId = apiContractBusinessContact.Contact_id,
                    ContactType = ContactType_ToModelContactTypeMap.Convert(apiContractBusinessContact.Contact_type)
                };
            }

            return modelBusinessContacts;
        }
    }
}