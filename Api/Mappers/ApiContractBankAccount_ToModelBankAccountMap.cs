using StateManagment.Entity;

namespace Api.Mappers
{
    public class ApiContractBankAccount_ToModelBankAccountMap
    {
        internal static BankAccount Update(ApiContract.UpdateBankAccount apiContractBankAccount, string legalEntityId)
        {
            if (apiContractBankAccount == null)
            {
                return null;
            }

            var modelBankAccount = new BankAccount()
            {
                BankAccountHolderNames = apiContractBankAccount.Account_holder_names != null ? [.. apiContractBankAccount.Account_holder_names] : null,
                AccountNumber = apiContractBankAccount.Account_number,
                BankCity = apiContractBankAccount.Bank_city,
                BankCountry = apiContractBankAccount.Bank_country,
                BankName = apiContractBankAccount.Bank_name,
                BillingDefault = apiContractBankAccount.Billing_default.HasValue ? apiContractBankAccount.Billing_default.Value : false,
                Iban = apiContractBankAccount.Iban,
                Name = apiContractBankAccount.Name,
                SortCode = apiContractBankAccount.Sort_code,
                Swift = apiContractBankAccount.Swift,
                LegalEntityId = legalEntityId
            };

            if (apiContractBankAccount.Labels != null)
            {
                modelBankAccount.Labels = [.. apiContractBankAccount.Labels];
            }

            if (apiContractBankAccount.Meta_data != null)
            {
                var metaDataList = new List<MetaDataModel>();
                foreach (var data in apiContractBankAccount.Meta_data)
                {
                    metaDataList.Add(new MetaDataModel { Key = data.Key, Value = data.Value });
                }
                modelBankAccount.MetaData = [.. metaDataList];
            }

            if (apiContractBankAccount.System_data != null)
            {
                var systemDataList = new List<SystemDataModel>();
                foreach (var data in apiContractBankAccount.System_data)
                {
                    systemDataList.Add(new SystemDataModel { Key = data.Key, Value = data.Value });
                }
                modelBankAccount.SystemData = [.. systemDataList];
            }

            return modelBankAccount;
        }
        internal static BankAccount Convert(ApiContract.CreateBankAccount apiContractBankAccount, string legalEntityId)
        {
            var modelBankAccount = new BankAccount()
            {
                BankAccountHolderNames = [.. apiContractBankAccount.Account_holder_names],
                AccountNumber = apiContractBankAccount.Account_number,
                BankCity = apiContractBankAccount.Bank_city,
                BankCountry = apiContractBankAccount.Bank_country,
                BankName = apiContractBankAccount.Bank_name,
                BillingDefault = apiContractBankAccount.Billing_default.HasValue ? apiContractBankAccount.Billing_default.Value : false,
                Iban = apiContractBankAccount.Iban,
                Name = apiContractBankAccount.Name,
                SortCode = apiContractBankAccount.Sort_code,
                Swift = apiContractBankAccount.Swift,
                LegalEntityId = legalEntityId
            };

            if (apiContractBankAccount.Labels != null)
            {
                modelBankAccount.Labels = [.. apiContractBankAccount.Labels];
            }

            if (apiContractBankAccount.Meta_data != null)
            {
                var metaDataList = new List<MetaDataModel>();
                foreach (var data in apiContractBankAccount.Meta_data)
                {
                    metaDataList.Add(new MetaDataModel { Key = data.Key, Value = data.Value });
                }
                modelBankAccount.MetaData = [.. metaDataList];
            }

            if (apiContractBankAccount.System_data != null)
            {
                var systemDataList = new List<SystemDataModel>();
                foreach (var data in apiContractBankAccount.System_data)
                {
                    systemDataList.Add(new SystemDataModel { Key = data.Key, Value = data.Value });
                }
                modelBankAccount.SystemData = [.. systemDataList];
            }

            return modelBankAccount;
        }
    }
}
