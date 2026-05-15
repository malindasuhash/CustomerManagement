using ApiContract;
using StateManagment.Entity;

namespace Api.Mappers
{
    internal class ControlTypes_ToModelControlTypesMap
    {
        internal static ControlType[] Convert(ICollection<PersonControlType> control_types)
        {
            if (control_types == null)
            {
                return null;
            }
            var modelControlTypes = new List<ControlType>();

            foreach ( var controlType in control_types )
            {
                switch (controlType)
                {
                    case PersonControlType.Shareholder:
                        modelControlTypes.Add(ControlType.Shareholder);
                        break;
                    case PersonControlType.Director:
                        modelControlTypes.Add(ControlType.Director);
                        break;
                    case PersonControlType.SignatoryBankInstructions:
                        modelControlTypes.Add(ControlType.BankInstructionSignatory);
                        break;
                    case PersonControlType.SignatoryContracts:
                        modelControlTypes.Add(ControlType.ContractSignatory);
                        break;
                }
            }

            return modelControlTypes.ToArray();
        }
    }
}