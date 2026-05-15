
namespace Api.Mappers
{
    internal class Address_ToModelAddressesMap
    {
        internal static StateManagment.Entity.Address Convert(ApiContract.Address address)
        {
            if (address == null)
            {
                return null;
            }

            var modelAddress = new StateManagment.Entity.Address()
            {
                Line1 = address.Line1,
                Line2 = address.Line2,
                Line3 = address.Line3,
                City = address.City,
                Code = address.Code,
                Country = address.Country,
                Locality = address.Locality,
                Name = address.Name,
                Region = address.Region
            };

            return modelAddress;
        }
    }
}