
namespace Api.Mappers
{
    internal class ApiContractRegisteredAddresses_ToModelRegisteredAddressesMap
    {
        internal static StateManagment.Entity.RegisteredAddress[] Convert(ICollection<ApiContract.RegisteredAddress> registered_addresses)
        {
            if (registered_addresses == null)
            {
                return null;
            }
            var result = new List<StateManagment.Entity.RegisteredAddress>();
            foreach (var registered_address in registered_addresses)
            {
                result.Add(ApiContractRegisteredAddress_ToModelAddressMap.Convert(registered_address));
            }
            return result.ToArray();
        }
    }
}