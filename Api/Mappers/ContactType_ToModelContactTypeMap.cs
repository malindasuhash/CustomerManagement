using ApiContract;
using StateManagment.Entity;

namespace Api.Mappers
{
    internal class ContactType_ToModelContactTypeMap
    {
        internal static ContactType Convert(BusinessContactType contact_type)
        {
            switch (contact_type)
            {
                case BusinessContactType.Financial:
                    return ContactType.Financial;
                case BusinessContactType.Technical:
                    return ContactType.Technical;
                case BusinessContactType.Developer:
                    return ContactType.Developer;
                case BusinessContactType.User:
                    return ContactType.User;
                case BusinessContactType.Customer:
                    return ContactType.Customer;
                default:
                    return ContactType.None;
            }
        }
    }
}