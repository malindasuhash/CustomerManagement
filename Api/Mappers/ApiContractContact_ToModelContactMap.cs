using StateManagment.Entity;

namespace Api.Mappers
{
    public class ApiContractContact_ToModelContactMap
    {
        public static Contact Convert(ApiContract.CreateContact apiContractContact)
        {
            if (apiContractContact == null) { return null; }

            var modelContact = new Contact()
            {
                Name = apiContractContact.Name,
                Email = apiContractContact.Email,
                AltTelephone = apiContractContact.Alt_telephone,
                AltTelephoneCode = apiContractContact.Alt_telephone_code,
                Telephone = apiContractContact.Telephone,
                TelephoneCode = apiContractContact.Telephone_code,
                PostalAddress = Address_ToModelAddressMap.Convert(apiContractContact.Postal_address)
            };
            if (apiContractContact.Labels != null)
            {
                modelContact.Labels = [.. apiContractContact.Labels];
            }
            if (apiContractContact.Meta_data != null)
            {
                var metaDataList = new List<MetaDataModel>();
                foreach (var data in apiContractContact.Meta_data)
                {
                    metaDataList.Add(new MetaDataModel { Key = data.Key, Value = data.Value });
                }
                modelContact.MetaData = [.. metaDataList];
            }
            return modelContact;
        }

        internal static Contact Update(ApiContract.UpdateContact patch)
        {
            if (patch == null) { return null; }

            var modelContact = new Contact()
            {
                Name = patch.Name,
                Email = patch.Email,
                AltTelephone = patch.Alt_telephone,
                AltTelephoneCode = patch.Alt_telephone_code,
                Telephone = patch.Telephone,
                TelephoneCode = patch.Telephone_code,
                PostalAddress = Address_ToModelAddressMap.Convert(patch.Postal_address)
            };
            if (patch.Labels != null)
            {
                modelContact.Labels = [.. patch.Labels];
            }
            if (patch.Meta_data != null)
            {
                var metaDataList = new List<MetaDataModel>();
                foreach (var data in patch.Meta_data)
                {
                    metaDataList.Add(new MetaDataModel { Key = data.Key, Value = data.Value });
                }
                modelContact.MetaData = [.. metaDataList];
            }
            return modelContact;
        }
    }
}
