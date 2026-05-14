using StateManagment.Entity;

namespace Api.Mappers
{
    public class ApiContractBillingGroup_ToModelBillingGroupMap
    {
        public static BillingGroup Convert(ApiContract.CreateBillingGroup apiContractBillingGroup)
        {
            var modelBillingGroup = new BillingGroup()
            {
                Name = apiContractBillingGroup.Name,
                Description = apiContractBillingGroup.Description,
                LegalEntityId = apiContractBillingGroup.Legal_entity_id,
                DirectDebitReference = apiContractBillingGroup.Direct_debit_reference,
                StatementNarrative = apiContractBillingGroup.Statement_narrative,
                BillingBankAccountId = apiContractBillingGroup.Billing_bank_account_id
            };
            if (apiContractBillingGroup.Labels != null)
            {
                modelBillingGroup.Labels = [.. apiContractBillingGroup.Labels];
            }
            if (apiContractBillingGroup.Meta_data != null)
            {
                var metaDataList = new List<MetaDataModel>();
                foreach (var data in apiContractBillingGroup.Meta_data)
                {
                    metaDataList.Add(new MetaDataModel { Key = data.Key, Value = data.Value });
                }
                modelBillingGroup.MetaData = [.. metaDataList];
            }
            if (apiContractBillingGroup.System_data != null)
            {
                var systemDataList = new List<SystemDataModel>();
                foreach (var data in apiContractBillingGroup.System_data)
                {
                    systemDataList.Add(new SystemDataModel { Key = data.Key, Value = data.Value });
                }
                modelBillingGroup.SystemData = [.. systemDataList];
            }
            return modelBillingGroup;
        }

        internal static BillingGroup Update(ApiContract.BillingGroup patch)
        {
            if (patch == null)
            {
                return null;
            }

            var modelBillingGroup = new BillingGroup()
            {
                Name = patch.Name,
                Description = patch.Description,
                LegalEntityId = patch.Legal_entity_id,
                DirectDebitReference = patch.Direct_debit_reference,
                StatementNarrative = patch.Statement_narrative,
                BillingBankAccountId = patch.Billing_bank_account_id
            };
            if (patch.Labels != null)
            {
                modelBillingGroup.Labels = [.. patch.Labels];
            }
            if (patch.Meta_data != null)
            {
                var metaDataList = new List<MetaDataModel>();
                foreach (var data in patch.Meta_data)
                {
                    metaDataList.Add(new MetaDataModel { Key = data.Key, Value = data.Value });
                }
                modelBillingGroup.MetaData = [.. metaDataList];
            }
            if (patch.System_data != null)
            {
                var systemDataList = new List<SystemDataModel>();
                foreach (var data in patch.System_data)
                {
                    systemDataList.Add(new SystemDataModel { Key = data.Key, Value = data.Value });
                }
                modelBillingGroup.SystemData = [.. systemDataList];
            }

            return modelBillingGroup;
        }
    }
}
