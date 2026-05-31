namespace Api.Mappers
{
    public class CustomerProfile_ToApiContractEntityResponseCustomer
    {
        internal static ApiContract.EntityResponse_Customer Convert(StateManagment.Models.CustomerProfile customerProfile)
        {
            if (customerProfile == null)
            {
                return null;
            }

            var apiContractCustomer = new ApiContract.EntityResponse_Customer()
            {
                Customer_id = customerProfile.CustomerId,
                Name = customerProfile.Name,
                Labels = customerProfile.Labels,
                Created_by = customerProfile.CreatedUser,
                Created = customerProfile.CreatedTimestamp.ToString(),
                Updated = customerProfile.UpdateTimestamp.ToString(),
                Updated_by = customerProfile.UpdateUser
            };

            if (customerProfile.MetaData != null)
            {
                var metaDataList = new ApiContract.MetaData();
                foreach (var data in customerProfile.MetaData)
                {
                    metaDataList.Add(data.Key, data.Value);
                }
                apiContractCustomer.Meta_data = metaDataList;
            }

            if (customerProfile.SystemData != null)
            {
                var systemDataList = new ApiContract.SystemData();
                foreach (var data in customerProfile.SystemData)
                {
                    systemDataList.Add(data.Key, data.Value);
                }
                apiContractCustomer.System_data = systemDataList;
            }

            return apiContractCustomer;
        }
    }
}
