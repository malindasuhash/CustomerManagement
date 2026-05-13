namespace Api.Mappers
{
    public class ApiContractAddress_ToModelAddressMap
    {
        internal static StateManagment.Entity.Address Convert(ApiContract.Address apiContractAddress)
        {
            if (apiContractAddress == null)
            {
                return null;
            }

            return new StateManagment.Entity.Address()
            {
                City = apiContractAddress.City,
                Code = apiContractAddress.Code,
                Country = apiContractAddress.Country,
                Line1 = apiContractAddress.Line1,
                Line2 = apiContractAddress.Line2,
                Line3 = apiContractAddress.Line3,
                Locality = apiContractAddress.Locality,
                Region = apiContractAddress.Region,
                Name = apiContractAddress.Name
            };
        }
    }
}