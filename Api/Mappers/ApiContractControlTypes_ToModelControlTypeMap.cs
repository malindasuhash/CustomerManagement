using StateManagment.Entity;

namespace Api.Mappers
{
    internal class ApiContractControlTypes_ToModelControlTypeMap
    {
        internal static ControlType[] Convert(ICollection<ApiContract.PersonControlType> control_types)
        {
                if (control_types == null)
                {
                    return null;
                }
    
                var result = new List<ControlType>();
    
                foreach (var controlType in control_types)
                {
                    switch (controlType)
                    {
                        case ApiContract.PersonControlType.Director:
                            result.Add(ControlType.Director);
                            break;
                        case ApiContract.PersonControlType.Shareholder:
                            result.Add(ControlType.Shareholder);
                            break;
                        case ApiContract.PersonControlType.SignatoryBankInstructions:
                            result.Add(ControlType.BankInstructionSignatory);
                            break;
                        case ApiContract.PersonControlType.SignatoryContracts:
                            result.Add(ControlType.ContractSignatory);
                            break;
                    }
            }

            return result.ToArray();
        }
    }
}