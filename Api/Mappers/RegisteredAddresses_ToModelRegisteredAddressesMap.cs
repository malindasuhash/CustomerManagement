
namespace Api.Mappers
{
    internal class RegisteredAddresses_ToModelRegisteredAddressesMap
    {
        internal static StateManagment.Entity.RegisteredAddress[] Convert(ICollection<ApiContract.RegisteredAddress> registered_addresses)
        {
            if (registered_addresses == null)
            {
                return null;
            }

            var convertedRegisteredAddresses = new StateManagment.Entity.RegisteredAddress[registered_addresses.Count];

            foreach (var registeredAddress in registered_addresses)
            {
                var convertedRegisteredAddress = new StateManagment.Entity.RegisteredAddress()
                {
                    Current = registeredAddress.Current,
                    DateFrom = registeredAddress.Date_from.Date,
                    DateTo = registeredAddress.Date_to.HasValue ? registeredAddress.Date_to.Value.Date : null,
                    Address = Address_ToModelAddressMap.Convert(registeredAddress.Address)
                };

                convertedRegisteredAddresses[registered_addresses.ToList().IndexOf(registeredAddress)] = convertedRegisteredAddress;
            }

            return convertedRegisteredAddresses;
        }
    }
}