
namespace Api.Mappers
{
    internal class ApiContractRegisteredAddress_ToModelAddressMap
    {
        internal static StateManagment.Entity.RegisteredAddress Convert(ApiContract.RegisteredAddress registeredAddress)
        {
            if (registeredAddress == null)
            {
                return null;
            }

            var convertedAddress = new StateManagment.Entity.RegisteredAddress()
            {
                Address = ApiContractAddress_ToModelAddressMap.Convert(registeredAddress.Address),
                Current = registeredAddress.Current,
                DateFrom = registeredAddress.Date_from == DateTimeOffset.MinValue ? (DateTime?)null : registeredAddress.Date_from.DateTime,
                DateTo = registeredAddress.Date_to.HasValue? registeredAddress.Date_to.Value.DateTime : null,
            };

            return convertedAddress;
        }
    }
}