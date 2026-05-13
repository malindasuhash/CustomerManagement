using Api.ApiModels;
using StateManagment.Models;

namespace Api.Mappers
{
    internal class MessageEnvelop_ToEntityResponse_TradingLocation
    {
        internal static ApiContract.EntityResponse_TradingLocation Convert(MessageEnvelop result)
        {
            return new ApiContract.EntityResponse_TradingLocation()
            {
                Customer = result.CustomerId,
                Id = result.EntityId,

                Draft = TradingLocation_ToApiContractMap.Convert(result.Draft),
                Draft_version = (long)result.DraftVersion,

                Submitted = TradingLocation_ToApiContractMap.Convert(result.Submitted),
                Submitted_version = (long)result.SubmittedVersion,

                Applied = TradingLocation_ToApiContractMap.Convert(result.Applied),
                Applied_version = (long)result.AppliedVersion,

                Created = result.CreatedTimestamp.ToString(),
                Created_by = result.CreatedUser,

                Updated = result.UpdateTimestamp.ToString(),
                Updated_by = result.UpdateUser,

                State = EntityState_ToApiStateMap.Convert(result.State),
                Feedback = result.Feedback != null ? result.Feedback.Select(f => new ApiContract.EntityStateResult
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