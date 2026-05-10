using StateManagment.Entity;
using StateManagment.Models;
using System.Net.NetworkInformation;

namespace Api.ApiModels
{
    public class BankAccountModel
    {
        public string[] BankAccountHolderNames { get; set; }
        public string AccountNumber { get; set; }
        public string BankCity { get; set; }
        public string BankCountry { get; set; }
        public string BankName { get; set; }
        public bool BillingDefault { get; set; }
        public string Iban { get; set; }
        public string Name { get; set; }
        public string SortCode { get; set; }
        public string Swift { get; set; }
        public MetaData[] MetaData { get; set; } = [];
        public string[] Labels { get; set; }
        public int TargetVersion { get; set; }
    }

    public class ApiContactBillingGroup_ToModelBillingGroupMap
    {
        public static BillingGroup Convert(ApiContract.CreateBillingGroup apiContractBillingGroup)
        {
            var modelBillingGroup = new BillingGroup()
            {
                Name = apiContractBillingGroup.Name,
                Description = apiContractBillingGroup.Description,
                LegalEntityId = apiContractBillingGroup.Legal_entity_id
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
    }

    public class ApiContractBankAccount_ToModelBankAccountMap
    {
        public static BankAccount Convert(ApiContract.CreateUpdateBankAccount apiContractBankAccount, string legalEntityId)
        {
            var modelBankAccount = new BankAccount()
            {
                BankAccountHolderNames = [.. apiContractBankAccount.Account_holder_names],
                AccountNumber = apiContractBankAccount.Account_number,
                BankCity = apiContractBankAccount.Bank_city,
                BankCountry = apiContractBankAccount.Bank_country,
                BankName = apiContractBankAccount.Bank_name,
                BillingDefault = true, // Billing default is set to true for new bank accounts
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

    public class  MessageEnvelop_ToEntityResponse_BillingGroup
    {
        public static ApiContract.EntityResponse_BillingGroup Convert(MessageEnvelop messageEnvelop)
        {
            return new ApiContract.EntityResponse_BillingGroup()
            {
                Customer = messageEnvelop.CustomerId,
                Id = messageEnvelop.EntityId,
                Draft = BillingGroup_ToApiContractMap.Convert(messageEnvelop.Draft), 
                Draft_version = (long)messageEnvelop.DraftVersion,
                Submitted = BillingGroup_ToApiContractMap.Convert(messageEnvelop.Submitted), 
                Submitted_version = (long)messageEnvelop.SubmittedVersion,
                Applied = BillingGroup_ToApiContractMap.Convert(messageEnvelop.Applied),
                Applied_version = (long)messageEnvelop.AppliedVersion,
                Created = messageEnvelop.CreatedTimestamp.ToString(),
                Created_by = messageEnvelop.CreatedUser,
                Updated = messageEnvelop.UpdateTimestamp.ToString(),
                Updated_by = messageEnvelop.UpdateUser,
                State = EntityState_ToApiStateMap.Convert(messageEnvelop.State),
                Feedback = messageEnvelop.Feedback != null ? messageEnvelop.Feedback.Select(f => new ApiContract.EntityStateResult
                {
                    Kind = FeedbackType_ToApiEntityStateKindMap.Convert(f.Type),
                    Message = f.Message,
                    Context = f.Context,
                    Details = f.Details
                }).ToArray() : null
             };
        }
    }

    public class BillingGroup_ToApiContractMap
    {
        public static ApiContract.BillingGroup Convert(BillingGroup billingGroupStateModel)
        {
            var responseBillingGroup = new ApiContract.BillingGroup()
            {
                Name = billingGroupStateModel.Name,
                Description = billingGroupStateModel.Description,
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

    public class MessageEnvelop_ToEntityResponse_BankAccount
    {
        public static ApiContract.EntityResponse_BankAccount Convert(MessageEnvelop entityDocument)
        {
            return new ApiContract.EntityResponse_BankAccount()
            {
                Customer = entityDocument.CustomerId,
                Id = entityDocument.EntityId,

                Draft = BankAccount_ToApiContractMap.Convert(entityDocument.Draft),
                Draft_version = (long)entityDocument.DraftVersion,

                Submitted = BankAccount_ToApiContractMap.Convert(entityDocument.Submitted),
                Submitted_version = (long)entityDocument.SubmittedVersion,

                Applied = BankAccount_ToApiContractMap.Convert(entityDocument.Applied),
                Applied_version = (long)entityDocument.AppliedVersion,

                Created = entityDocument.CreatedTimestamp.ToString(),
                Created_by = entityDocument.CreatedUser,

                Updated = entityDocument.UpdateTimestamp.ToString(),
                Updated_by = entityDocument.UpdateUser,

                State = EntityState_ToApiStateMap.Convert(entityDocument.State),
                Feedback = entityDocument.Feedback != null ? entityDocument.Feedback.Select(f => new ApiContract.EntityStateResult
                {
                    Kind = FeedbackType_ToApiEntityStateKindMap.Convert(f.Type),
                    Message = f.Message,
                    Context = f.Context,
                    Details = f.Details
                }).ToArray() : null
            };
        }
    }

    public class FeedbackType_ToApiEntityStateKindMap
    {
        public static ApiContract.EntityStateResultKind Convert(StateManagment.Models.FeedbackType feedbackType)
        {
            return feedbackType switch
            {
                StateManagment.Models.FeedbackType.DocumentRequired => ApiContract.EntityStateResultKind.DocumentRequired,
                StateManagment.Models.FeedbackType.WaitingForExternalRiskChecks => ApiContract.EntityStateResultKind.WaitingForExternalRiskChecks,
                StateManagment.Models.FeedbackType.LegalEntityMissing => ApiContract.EntityStateResultKind.LegalEntityMissing,
                StateManagment.Models.FeedbackType.WaitingForProductSelection => ApiContract.EntityStateResultKind.WaitingForProductSelection,
                StateManagment.Models.FeedbackType.MissingRequiredInformation => ApiContract.EntityStateResultKind.MissingRequiredInformation,
                StateManagment.Models.FeedbackType.InternalError => ApiContract.EntityStateResultKind.InternalError,
                StateManagment.Models.FeedbackType.WaitingForContractSignatureOrAcceptance => ApiContract.EntityStateResultKind.WaitingForContractSignatureOrAcceptance,
                StateManagment.Models.FeedbackType.UserActionRequired => ApiContract.EntityStateResultKind.UserActionRequired,
                StateManagment.Models.FeedbackType.WaitingForLegalEntityApproval => ApiContract.EntityStateResultKind.WaitingForLegalEntityApproval,
                StateManagment.Models.FeedbackType.WaitingForConfiguration => ApiContract.EntityStateResultKind.WaitingForConfiguration,

                _ => throw new ArgumentOutOfRangeException(nameof(feedbackType), $"Not expected feedback type value: {feedbackType}")
            };
        }
    }

    public class EntityState_ToApiStateMap
    {
        public static ApiContract.EntityState Convert(StateManagment.Models.EntityState entityState)
        {
            return entityState switch
            {
                StateManagment.Models.EntityState.NEW => ApiContract.EntityState.New,
                StateManagment.Models.EntityState.EVALUATING => ApiContract.EntityState.Evaluating,
                StateManagment.Models.EntityState.EVALUATION_RESTARTING => ApiContract.EntityState.EvaluationRestarting,
                StateManagment.Models.EntityState.ATTENTION_REQUIRED => ApiContract.EntityState.AttentionRequired,
                StateManagment.Models.EntityState.IN_REVIEW => ApiContract.EntityState.InReview,
                StateManagment.Models.EntityState.IN_PROGRESS => ApiContract.EntityState.InProgress,
                StateManagment.Models.EntityState.SYNCHRONISED => ApiContract.EntityState.Synchronised,

                _ => throw new ArgumentOutOfRangeException(nameof(entityState), $"Not expected entity state value: {entityState}")
            };
        }
    }

    public class BankAccount_ToApiContractMap
    {
        public static ApiContract.BankAccount Convert(BankAccount bankAccountStateModel)
        {
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

            return responseBankAccount;
        }
    }
}
