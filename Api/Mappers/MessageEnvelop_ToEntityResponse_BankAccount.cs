using StateManagment.Models;

namespace Api.Mappers
{
    public class MessageEnvelop_ToEntityResponse_BankAccount
        {
            internal static ApiContract.EntityResponse_BankAccount Convert(MessageEnvelop entityDocument)
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
    }
