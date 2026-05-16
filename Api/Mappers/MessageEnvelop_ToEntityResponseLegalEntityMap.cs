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
            var legalEntity = new StateManagment.Entity.LegalEntity()
            {
                BusinessEmail = patch.Business_email,
                CardTurnoverPerAnnum = patch.Turnover_per_annum,
                CharityRegistration = patch.Charity_registration,
                CompanyRegistration = patch.Company_registration,
                CountryOfAuthority = patch.Country_of_authority,
                DateBusinessStarted = patch.Date_business_created == null ? (DateTime?)null : DateTime.Parse(patch.Date_business_created),
                DateTradingStarted = patch.Date_trading_started == null ? (DateTime?)null : DateTime.Parse(patch.Date_trading_started),
                GoodsOwnership = ApiContractGoodsOwnership_ToModelGoodsOwnershipType.Convert(patch.Goods_ownership),
                MaximumTransactionValue = patch.Maximum_transaction_value,
                Name = patch.Name,
                StandardIndustryClassification = patch.Standard_industry_classification,
                TradingIndustryClassification = patch.Trading_industry_classification,
                TradingName = patch.Trading_name,
                BusinessContacts = ApiContractBusinessContacts_ToModelBusinessContactsType.Convert(patch.Business_contacts),
                BusinessIdentification = patch.Business_identification,
                BusinessType = ApiContractBusinessType_ToModelBusinessTypeMap.Convert(patch.Business_type),
                //EndOfBusinessRelationship = ApiContractEndOfBusinessRelationship_ToModelEndOfBusinessRelationshipMap.Convert(patch.en),
                LegalEntitiesWithControl = ApiContractLegalEntitiesWithControl_ToModelLegalEntitiesWithControlMap.Convert(patch.Legal_entities_with_control),
                MerchantCategoryCode = patch.Merchant_category_code,
                PartnersWithInterest = ApiContractPartnersWithInterest_ToModelPartnersWithInterestMap.Convert(patch.Partners_with_interest),
                PersonsWithControl = ApiContractPersonsWithControl_ToModelPersonsWithControlMap.Convert(patch.Persons_with_control),
                RegisteredAddresses = ApiContractRegisteredAddresses_ToModelRegisteredAddressesMap.Convert(patch.Registered_addresses),
                TurnoverPerAnnum = patch.Turnover_per_annum,
                VatRegistration = patch.Vat_registration,
                VatRegistrationStatus = ApiContractVatRegistrationStatus_ToModelVatRegistrationStatusMap.Convert(patch.Vat_registration_status),
                // Status = patch.
            };

            if (patch.Labels != null)
            {
                legalEntity.Labels = [.. patch.Labels];
            }
            if (patch.Meta_data != null)
            {
                legalEntity.MetaData = patch.Meta_data.Select(md => new StateManagment.Entity.MetaDataModel() { Key = md.Key, Value = md.Value }).ToArray();
            }
            if (patch.System_data != null)
            {
                legalEntity.SystemData = patch.System_data.Select(sd => new StateManagment.Entity.SystemDataModel() { Key = sd.Key, Value = sd.Value }).ToArray();
            }

            return legalEntity;
        }
    }
}
