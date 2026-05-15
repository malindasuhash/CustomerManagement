
namespace Api.Mappers
{
    internal class ApiContractPersonsWithControl_ToModelPersonsWithControlMap
    {
        internal static StateManagment.Entity.PersonWithControl[] Convert(ICollection<ApiContract.PersonWithControl> persons_with_control)
        {
            if (persons_with_control == null)
            {
                return null;
            }

            var result = new List<StateManagment.Entity.PersonWithControl>();

            foreach (var person in persons_with_control)
            {
                result.Add(new StateManagment.Entity.PersonWithControl()
                {
                    ControlTypes = ApiContractControlTypes_ToModelControlTypeMap.Convert(person.Control_types),
                    Person = ApiContractPerson_ToModelPersonMap.Convert(person.Person)
                });
            }

            return result.ToArray();
        }
    }
}