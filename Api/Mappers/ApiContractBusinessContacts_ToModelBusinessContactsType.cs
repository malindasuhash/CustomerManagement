
namespace Api.Mappers
{
    internal class ApiContractBusinessContacts_ToModelBusinessContactsType
    {
        internal static StateManagment.Entity.BusinessContact[] Convert(ICollection<ApiContract.BusinessContact> business_contacts)
        {
            if (business_contacts == null)
            {
                return null;
            }

            var converted_business_contacts = new List<StateManagment.Entity.BusinessContact>();
            foreach (var business_contact in business_contacts)
            {
                if (business_contact == null)
                {
                    return null;
                }
                converted_business_contacts.Add(new StateManagment.Entity.BusinessContact()
                {
                    ContactId = business_contact.Contact_id,
                    ContactType = ApiContractBusinessContactType_ToModelBusinessContactTypeMap.Convert(business_contact.Contact_type),
                });
            }

            return converted_business_contacts.ToArray();
        }
    }
}