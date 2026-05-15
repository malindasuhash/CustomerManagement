
namespace Api.Mappers
{
    internal class Person_ToApiContractMap
    {
        internal static ApiContract.Person Convert(StateManagment.Entity.Person person)
        {
            if (person == null)
            {
                return null;
            }

            var apiPerson = new ApiContract.Person()
            {
                Date_of_birth = person.DateOfBirth?.ToString("yyyy-MM-dd"),
                First_name = person.FirstName,
                Last_name = person.LastName,
                Middle_name = person.MiddleName,
                Nationality = person.Nationality,
                Title = person.Title,
                Person_identification = person.PersonIdentification,
                Addresses = RegisteredAddresses_ToApiContractMap.Convert(person.Addresses),
            };

            return apiPerson;
        }
    }
}