
namespace Api.Mappers
{
    internal class ApiContractPerson_ToModelPersonMap
    {
        internal static StateManagment.Entity.Person Convert(ApiContract.Person person)
        {
            if (person == null)
            {
                return null;
            }

            var personWithControl = new StateManagment.Entity.Person()
            {
                DateOfBirth = person.Date_of_birth == null ? (DateTime?)null : DateTime.Parse(person.Date_of_birth),
                FirstName = person.First_name,
                LastName = person.Last_name,
                Title = person.Title,
                Nationality = person.Nationality,
                MiddleName = person.Middle_name,
            };

            if (person.Addresses != null)
            {
                personWithControl.Addresses = new StateManagment.Entity.RegisteredAddress[person.Addresses.Count];
                for (int i = 0; i < person.Addresses.Count; i++)
                {
                    personWithControl.Addresses[i] = ApiContractRegisteredAddress_ToModelAddressMap.Convert(person.Addresses.ElementAt(i));
                }
            }

            return personWithControl;

        }
    }
}