using ApiContract;
using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Mappers
{
    public class ApiContractUpdateCustomer_ToModelCustomerProfile
    {
        internal static CustomerProfile Convert(string customerId, UpdateCustomer customerProfile)
        {
            if (customerId == null) { return null; }

            var customer = new CustomerProfile
            {
                CustomerId = customerId,
                Name = customerProfile.Name
            };

            if (customerProfile.System_data != null)
            {
                var systemData = new List<SystemDataModel>();

                foreach (var data in customerProfile.System_data) 
                { 
                    systemData.Add(new SystemDataModel() { Key = data.Key, Value = data.Value  });
                }

                customer.SystemData = systemData.ToArray();
            }

            if (customerProfile.Meta_data != null)
            {
                var metaData = new List<MetaDataModel>();

                foreach (var data in customerProfile.Meta_data)
                {
                    metaData.Add(new MetaDataModel() { Key = data.Key, Value = data.Value });
                }

                customer.MetaData = metaData.ToArray();
            }

            customer.UpdateTimestamp = DateTime.Now;
            customer.UpdateUser = "SYSTEM2"; // TODO: Resolve from Request

            return customer;
        }
    }
}
