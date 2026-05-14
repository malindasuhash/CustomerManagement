using StateManagment.Entity;

namespace Api.ApiModels
{
    public class BillingGroup_ToApiContractMap
    {
        public static ApiContract.BillingGroup Convert(BillingGroup billingGroupStateModel)
        {
            if (billingGroupStateModel == null)
            {
                return null;
            }
            var responseBillingGroup = new ApiContract.BillingGroup()
            {
                Name = billingGroupStateModel.Name,
                Description = billingGroupStateModel.Description,
                Billing_bank_account_id = billingGroupStateModel.BillingBankAccountId,
                Legal_entity_id = billingGroupStateModel.LegalEntityId,
                Direct_debit_reference = billingGroupStateModel.DirectDebitReference,
                Statement_narrative = billingGroupStateModel.StatementNarrative,
            };

            if (billingGroupStateModel.MetaData != null)
            {
                var metaData = new ApiContract.MetaData();
                foreach (var data in billingGroupStateModel.MetaData)
                {
                    metaData.Add(data.Key, data.Value);
                }
                responseBillingGroup.Meta_data = metaData;
            }

            if (billingGroupStateModel.Labels != null)
            {
                var labels = new ApiContract.Labels();
                foreach (var label in billingGroupStateModel.Labels)
                {
                    labels.Add(label);
                }
                responseBillingGroup.Labels = labels;
            }

            if (billingGroupStateModel.SystemData != null)
            {
                var systemData = new ApiContract.SystemData();
                foreach (var data in billingGroupStateModel.SystemData)
                {
                    systemData.Add(data.Key, data.Value);
                }
                responseBillingGroup.System_data = systemData;
            }

            return responseBillingGroup;
        }
    }
}
