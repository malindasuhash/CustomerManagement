
namespace Api.Mappers
{
    internal class ApiContractBusinessType_ToModelBusinessTypeMap
    {
        internal static StateManagment.Entity.BusinessType Convert(ApiContract.BusinessType business_type)
        {
            switch (business_type)
            {
                case ApiContract.BusinessType.SoleTrader:
                    return StateManagment.Entity.BusinessType.SoleProprietorship;
                case ApiContract.BusinessType.Partnership:
                    return StateManagment.Entity.BusinessType.Partnership;
                case ApiContract.BusinessType.Company:
                    return StateManagment.Entity.BusinessType.Corporation;
                case ApiContract.BusinessType.Standalone_FCA_Regulated:
                    return StateManagment.Entity.BusinessType.LimitedLiabilityCompany;
            }
            return StateManagment.Entity.BusinessType.Other;
        }
    }
}