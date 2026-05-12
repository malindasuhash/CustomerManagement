using StateManagment.Entity;

namespace Api.ApiModels
{
    public class ApiContractContact_ToModelContactMap
    {
        public static Contact Convert(ApiContract.CreateContact apiContractContact)
        {
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
    }
    }
