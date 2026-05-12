using StateManagment.Entity;

namespace Api.Mappers
{
    public class BankAccount_ToApiContractMap
    {
        public static ApiContract.BankAccount Convert(BankAccount bankAccountStateModel)
        {
            if (bankAccountStateModel == null)
            {
                return null;
            }

            var responseBankAccount = new ApiContract.BankAccount()
            {
                Account_holder_names = bankAccountStateModel.BankAccountHolderNames,
                Account_number = bankAccountStateModel.AccountNumber,
                Bank_city = bankAccountStateModel.BankCity,
                Bank_country = bankAccountStateModel.BankCountry,
                Bank_name = bankAccountStateModel.BankName,
                Billing_default = bankAccountStateModel.BillingDefault,
                Iban = bankAccountStateModel.Iban,
                Name = bankAccountStateModel.Name,
                Sort_code = bankAccountStateModel.SortCode,
                Swift = bankAccountStateModel.Swift,
            };

            if (bankAccountStateModel.Labels != null)
            {
                var labels = new ApiContract.Labels();
                foreach (var label in bankAccountStateModel.Labels)
                {
                    labels.Add(label);
                }
                responseBankAccount.Labels = labels;
            }

            if (bankAccountStateModel.MetaData != null)
            {
                var metaData = new ApiContract.MetaData();
                foreach (var data in bankAccountStateModel.MetaData)
                {
                    metaData.Add(data.Key, data.Value);
                }
                responseBankAccount.Meta_data = metaData;
            }

            if (bankAccountStateModel.SystemData != null)
            {
                var systemData = new ApiContract.SystemData();
                foreach (var data in bankAccountStateModel.SystemData)
                {
                    systemData.Add(data.Key, data.Value);
                }
                responseBankAccount.System_data = systemData;
            }

            return responseBankAccount;
        }
    }
}
