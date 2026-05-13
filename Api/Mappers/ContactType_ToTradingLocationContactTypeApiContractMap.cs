using StateManagment.Entity;

namespace Api.Mappers
{
    public class ContactType_ToTradingLocationContactTypeApiContractMap
    {
        public static ApiContract.TradingLocationContactType Convert(ContactType contactType)
        {
            return contactType switch
            {
                ContactType.Manager => ApiContract.TradingLocationContactType.Manager,
                ContactType.Support => ApiContract.TradingLocationContactType.Support,
                ContactType.Technical => ApiContract.TradingLocationContactType.Technical,
                _ => throw new ArgumentOutOfRangeException(nameof(contactType), $"Not expected contact type value: {contactType}"),
            };
        }
    }
}