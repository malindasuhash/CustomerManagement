
namespace Api.Mappers
{
    internal class Person_ToModelPersonMap
    {
        internal static StateManagment.Entity.Person Convert(ApiContract.Person person)
        {
            if (person == null)
            {
                return null;
            }

            var modelPerson = new StateManagment.Entity.Person()
            {
                Title = person.Title,
                FirstName = person.First_name,
                LastName = person.Last_name,
                MiddleName = person.Middle_name,
                DateOfBirth = person.Date_of_birth != null ? DateTime.Parse(person.Date_of_birth) : null,
                Nationality = person.Nationality,
                Addresses = RegisteredAddresses_ToModelAddressesMap.Convert(person.Addresses),
                PersonIdentification = person.Person_identification,
            };

            return modelPerson;
        }
    }
}