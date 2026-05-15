using ApiContract;
using StateManagment.Entity;

namespace Api.Mappers
{
    internal class ApiControlTypes_ToControlTypesMap
    {
        internal static ControlType[] Convert(ICollection<LegalEntityControlType> control_types)
        {
            if (control_types == null)
            {
                return null;
            }
            var modelControlTypes = new List<ControlType>();

            foreach (var controlType in control_types)
            {
                switch (controlType)
                {
                    case LegalEntityControlType.Shareholder:
                        modelControlTypes.Add(ControlType.Shareholder);
                        break;
                    case LegalEntityControlType.ParentCompany:
                        modelControlTypes.Add(ControlType.ParentCompany);
                        break;
                }
            }

            return modelControlTypes.ToArray();
        }
    }
}