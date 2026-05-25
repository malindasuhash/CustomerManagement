using StateManagment.Entity;

namespace Api.Mappers
{
    public static class ApiContractCreateCustomer_ToModelCustomerProfile
    {
        internal static StateManagment.Models.CustomerProfile Convert(ApiContract.CreateCustomer createCustomer)
        {
            var profile = new StateManagment.Models.CustomerProfile
            {
                Name = createCustomer.Name,
                CustomerId = createCustomer.Customer_id
            };

            if (createCustomer.Labels != null)
            {
                profile.Labels = createCustomer.Labels.ToArray();
            }

            if (createCustomer.Meta_data != null)
            {
                var metaDataList = new List<MetaDataModel>();
                foreach (var data in createCustomer.Meta_data)
                {
                    metaDataList.Add(new MetaDataModel { Key = data.Key, Value = data.Value });
                }
                profile.MetaData = [.. metaDataList];
            }

            if (createCustomer.System_data != null)
            {
                var systemDataList = new List<SystemDataModel>();
                foreach (var data in createCustomer.System_data)
                {
                    systemDataList.Add(new SystemDataModel { Key = data.Key, Value = data.Value });
                }
                profile.SystemData = [.. systemDataList];
            }

            return profile;
        }
    }
}
