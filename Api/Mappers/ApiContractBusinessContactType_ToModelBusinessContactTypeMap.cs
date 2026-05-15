using ApiContract;
using StateManagment.Entity;

namespace Api.Mappers
{
    internal class ApiContractBusinessContactType_ToModelBusinessContactTypeMap
    {
        internal static ContactType Convert(BusinessContactType contact_type)
        {
            switch (contact_type)
            {
                case BusinessContactType.Financial:
                    return ContactType.Financial;
                case BusinessContactType.Technical:
                    return ContactType.Technical;
                case BusinessContactType.Customer:
                    return ContactType.Customer;
                case BusinessContactType.Developer:
                    return ContactType.Developer;
                case BusinessContactType.User:
                    return ContactType.User;
            }

            return ContactType.None;
        }
    }
}