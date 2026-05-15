using StateManagment.Entity;

namespace Api.Mappers
{
    internal class LegalEntitiesWithControl_ToModelLegalEntitiesWithControlMap
    {
        internal static LegalEntityWithControl[] Convert(ICollection<ApiContract.LegalEntityWithControl> legal_entities_with_control)
        {
            if (legal_entities_with_control == null)
            {
                return null;
            }

            var modelLegalEntitiesWithControl = new List<LegalEntityWithControl>();

            foreach (var control in legal_entities_with_control)
            {
                modelLegalEntitiesWithControl.Add(new LegalEntityWithControl()
                {
                    LegalEntityId = control.Legal_entity_id,
                    ControlTypes = ApiControlTypes_ToControlTypesMap.Convert(control.Control_types),
                });
            }

            return modelLegalEntitiesWithControl.ToArray();
        }
    }
}