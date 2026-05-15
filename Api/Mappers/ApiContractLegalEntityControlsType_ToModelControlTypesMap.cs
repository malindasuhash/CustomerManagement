using ApiContract;
using StateManagment.Entity;

namespace Api.Mappers
{
    internal class ApiContractLegalEntityControlsType_ToModelControlTypesMap
    {
        internal static ControlType[] Convert(ICollection<LegalEntityControlType> control_types)
        {
            if (control_types == null)
            {
                return null;
            }

            var controlTypes = new List<ControlType>();
            foreach (var controlType in control_types)
            {
                switch (controlType)
                {
                    case LegalEntityControlType.ParentCompany:
                        controlTypes.Add(ControlType.ParentCompany);
                        break;
                    case LegalEntityControlType.Shareholder:
                        controlTypes.Add(ControlType.Shareholder);
                        break;
                }
            }

            return controlTypes.ToArray();
        }
    }
}