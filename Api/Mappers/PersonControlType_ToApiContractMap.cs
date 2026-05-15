using ApiContract;
using StateManagment.Entity;

namespace Api.Mappers
{
    internal class PersonControlType_ToApiContractMap
    {
        internal static ICollection<PersonControlType> Convert(ControlType[] controlTypes)
        {
            if (controlTypes == null)
            {
                return null;
            }

            var personControlTypes = new List<PersonControlType>();

            foreach (var controlType in controlTypes)
            {
                switch (controlType)
                {
                    case ControlType.Shareholder:
                        personControlTypes.Add(PersonControlType.Shareholder);
                        break;
                    case ControlType.Director:
                        personControlTypes.Add(PersonControlType.Director);
                        break;
                    case ControlType.BankInstructionSignatory:
                        personControlTypes.Add(PersonControlType.SignatoryBankInstructions);
                        break;
                    case ControlType.ContractSignatory:
                        personControlTypes.Add(PersonControlType.SignatoryContracts);
                        break;
                }
            }

            return personControlTypes;
        }
    }
}