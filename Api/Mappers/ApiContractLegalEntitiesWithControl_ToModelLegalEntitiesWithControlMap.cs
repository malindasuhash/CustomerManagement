using StateManagment.Entity;

namespace Api.Mappers
{
    internal class ApiContractLegalEntitiesWithControl_ToModelLegalEntitiesWithControlMap
    {
        internal static LegalEntityWithControl[] Convert(ICollection<ApiContract.LegalEntityWithControl> legal_entities_with_control)
        {
            if (legal_entities_with_control == null)
            {
                return null;
            }

            var entitiesWithControl = new List<LegalEntityWithControl>();

            foreach (var entity in legal_entities_with_control)
            {
                entitiesWithControl.Add(new LegalEntityWithControl()
                {
                    LegalEntityId = entity.Legal_entity_id,
                    ControlTypes = ApiContractLegalEntityControlsType_ToModelControlTypesMap.Convert(entity.Control_types)
                });
            }

            return entitiesWithControl.ToArray();
        }
    }
}