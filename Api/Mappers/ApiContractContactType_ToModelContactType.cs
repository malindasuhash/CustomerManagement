namespace Api.Mappers
{
    internal class ApiContractContactType_ToModelContactType
    {
        public static StateManagment.Entity.ContactType Convert(ApiContract.TradingLocationContactType tradingLocationContactType)
        {
            return tradingLocationContactType switch
            {
                ApiContract.TradingLocationContactType.Manager => StateManagment.Entity.ContactType.Manager,
                ApiContract.TradingLocationContactType.Support => StateManagment.Entity.ContactType.Support,
               ApiContract.TradingLocationContactType.Technical => StateManagment.Entity.ContactType.Technical,
                _ => throw new ArgumentOutOfRangeException(nameof(tradingLocationContactType), $"Not expected contact type value: {tradingLocationContactType}")
            };
        }
    }
}