using StateManagment.Models;

namespace Api.Mappers
{
    public class MessageEnvelop_ToEntityResponseLegalEntityMap
    {
        public static ApiContract.EntityResponse_LegalEntity Convert(MessageEnvelop messageEnvelop)
        {
            return new ApiContract.EntityResponse_LegalEntity()
            {
                Customer = messageEnvelop.CustomerId,
                Id = messageEnvelop.EntityId,
                Draft = LegalEntity_ToApiContractMap.Convert(messageEnvelop.Draft),
                Draft_version = (long)messageEnvelop.DraftVersion,
                Submitted = LegalEntity_ToApiContractMap.Convert(messageEnvelop.Submitted),
                Submitted_version = (long)messageEnvelop.SubmittedVersion,
                Applied = LegalEntity_ToApiContractMap.Convert(messageEnvelop.Applied),
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

        internal static StateManagment.Entity.LegalEntity Convert(ApiContract.UpdateLegalEntity patch)
        {
            throw new NotImplementedException();
        }
    }
}
