
namespace Api.Mappers
{
    internal class ApiContractGoodsOwnership_ToModelGoodsOwnershipType
    {
        internal static StateManagment.Entity.GoodsOwnership Convert(ApiContract.GoodsOwnership goods_ownership)
        {
            switch (goods_ownership)
            {
                case ApiContract.GoodsOwnership.Owned:
                    return StateManagment.Entity.GoodsOwnership.Owned;
                case ApiContract.GoodsOwnership.Leased:
                    return StateManagment.Entity.GoodsOwnership.Leased;
                case ApiContract.GoodsOwnership.Other:
                    return StateManagment.Entity.GoodsOwnership.Other;
                case ApiContract.GoodsOwnership.ThirdParty:
                    return StateManagment.Entity.GoodsOwnership.ThirdParty;
            }
            return StateManagment.Entity.GoodsOwnership.Other;
        }
    }
}