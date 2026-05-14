using Microsoft.AspNetCore.Mvc;
using StateManagment.Models;

namespace Api.Mappers
{
    internal class MessageEnvelop_ToEntityResponse_ProductAgreement
    {
        internal static ApiContract.EntityResponse_ProductAgreement Convert(MessageEnvelop entityDocument)
        {
            return new ApiContract.EntityResponse_ProductAgreement()
            {
                Customer = entityDocument.CustomerId,
                Id = entityDocument.EntityId,

                Draft = ProductAgreement_ToApiContractMap.Convert(entityDocument.Draft),
                Draft_version = (long)entityDocument.DraftVersion,

                Submitted = ProductAgreement_ToApiContractMap.Convert(entityDocument.Submitted),
                Submitted_version = (long)entityDocument.SubmittedVersion,

                Applied = ProductAgreement_ToApiContractMap.Convert(entityDocument.Applied),
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