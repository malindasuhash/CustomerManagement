
namespace Api.Mappers
{
    internal class GoodsOwnership_ToModelGoodsOwnershipMap
    {
        internal static StateManagment.Entity.GoodsOwnership Convert(ApiContract.GoodsOwnership goods_ownership)
        {
            switch (goods_ownership)
            {
                case ApiContract.GoodsOwnership.Leased:
                    return StateManagment.Entity.GoodsOwnership.Leased;
                case ApiContract.GoodsOwnership.ThirdParty:
                    return StateManagment.Entity.GoodsOwnership.ThirdParty;
                case ApiContract.GoodsOwnership.Owned:
                    return StateManagment.Entity.GoodsOwnership.Owned;
                default:
                    return StateManagment.Entity.GoodsOwnership.Other;
            }
        }
    }
}