using StateManagment.Entity;

namespace Api.Mappers
{
    public class PersonsWithControl_ToApiContractMap
    {
        public static ApiContract.PersonWithControl[] Convert(PersonWithControl[] personsWithControl)
        {
            if (personsWithControl == null)
            {
                return null;
            }

            var personsWithControlList = new List<ApiContract.PersonWithControl>();

            foreach (var personWithControl in personsWithControl)
            {
                var apiPersonWithControl = new ApiContract.PersonWithControl()
                {
                   Person = Person_ToApiContractMap.Convert(personWithControl.Person),
                   Date_from = personWithControl.FromDate.HasValue ? personWithControl.FromDate.Value : null,
                   Date_to = personWithControl.ToDate.HasValue ? personWithControl.ToDate.Value : null,
                   Designation = personWithControl.Designation,
                   Ownership_percentage = personWithControl.OwnershipPercentage,
                   Control_types = PersonControlType_ToApiContractMap.Convert(personWithControl.ControlTypes),
                };

                personsWithControlList.Add(apiPersonWithControl);
            }

            return personsWithControlList.ToArray();
        }
    }
}
