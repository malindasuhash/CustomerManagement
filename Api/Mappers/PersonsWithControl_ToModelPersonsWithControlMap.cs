
namespace Api.Mappers
{
    internal class PersonsWithControl_ToModelPersonsWithControlMap
    {
        internal static StateManagment.Entity.PersonWithControl[] Convert(ICollection<ApiContract.PersonWithControl> persons_with_control)
        {
            if (persons_with_control == null)
            {
                return null;
            }

            var modelPersonsWithControl = new List<StateManagment.Entity.PersonWithControl>();

            foreach (var personWithControl in persons_with_control)
            {
                modelPersonsWithControl.Add(new StateManagment.Entity.PersonWithControl()
                {
                    Person = Person_ToModelPersonMap.Convert(personWithControl.Person),
                    ControlTypes = ControlTypes_ToModelControlTypesMap.Convert(personWithControl.Control_types)
                });
            }

            return modelPersonsWithControl.ToArray();
        }
    }
}