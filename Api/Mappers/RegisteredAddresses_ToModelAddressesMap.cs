
namespace Api.Mappers
{
    internal class RegisteredAddresses_ToModelAddressesMap
    {
        internal static StateManagment.Entity.RegisteredAddress[] Convert(ICollection<ApiContract.RegisteredAddress> addresses)
        {
            if (addresses == null)
            {
                return null;
            }

            var modelAddresses = new List<StateManagment.Entity.RegisteredAddress>();

            foreach (var address in addresses)
            {
                modelAddresses.Add(new StateManagment.Entity.RegisteredAddress()
                {
                    Current = address.Current,
                    DateFrom = address.Date_from.Date,
                    DateTo = address.Date_to.HasValue ? address.Date_to.Value.Date : null,
                    Address = Address_ToModelAddressesMap.Convert(address.Address),
                });
            }

            return modelAddresses.ToArray();
        }
    }
}