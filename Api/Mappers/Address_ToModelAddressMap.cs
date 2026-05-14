using StateManagment.Entity;
using StateManagment.Models;

namespace Api.Mappers
{
    public class Address_ToModelAddressMap
    {
        public static Address Convert(ApiContract.Address address)
        {
            if (address == null)
            {
                return null;
            }
            return new Address()
            {
                Line1 = address.Line1,
                Line2 = address.Line2,
                Line3 = address.Line3,
                Region = address.Region,
                Locality = address.Locality,
                Code = address.Code,
                Country = address.Country,
                City = address.City
            };
        }
    }

    public class BusinessType_ToApiContractMap
    {
        public static ApiContract.BusinessType Convert(BusinessType businessType)
        {
            return businessType switch
            {
                BusinessType.SoleProprietorship => ApiContract.BusinessType.SoleTrader,
                BusinessType.Partnership => ApiContract.BusinessType.Partnership,
                BusinessType.Corporation => ApiContract.BusinessType.Company,
                BusinessType.LimitedLiabilityCompany => ApiContract.BusinessType.Standalone_FCA_Regulated,
                _ => throw new ArgumentOutOfRangeException(nameof(businessType), $"Not expected business type value: {businessType}")
            };
        }
    }

    public class VatRegistrationStatus_ToApiContractMap
    {
        public static ApiContract.VatRegistrationStatus Convert(VatRegistrationStatus vatRegistrationStatus)
        {
            return vatRegistrationStatus switch
            {
                VatRegistrationStatus.Registered => ApiContract.VatRegistrationStatus.Registered,
                VatRegistrationStatus.NotRegistered => ApiContract.VatRegistrationStatus.NotRegistered,
                VatRegistrationStatus.Pending => ApiContract.VatRegistrationStatus.RegistrationPending,
                _ => throw new ArgumentOutOfRangeException(nameof(vatRegistrationStatus), $"Not expected VAT registration status value: {vatRegistrationStatus}")
            };
        }
    }

    public class LegalEntity_ToApiContractMap
    {
        public static ApiContract.LegalEntity Convert(LegalEntity legalEntityStateModel)
        {
            var responseLegalEntity = new ApiContract.LegalEntity()
            {
                Name = legalEntityStateModel.Name,
                Business_email = legalEntityStateModel.BusinessEmail,
                Business_type = BusinessType_ToApiContractMap.Convert(legalEntityStateModel.BusinessType),
                Date_business_created = legalEntityStateModel.DateBusinessStarted.ToString(),
                Date_trading_started = legalEntityStateModel.DateTradingStarted.ToString(),
                Charity_registration = legalEntityStateModel.CharityRegistration,
                Vat_registration = legalEntityStateModel.VatRegistration,
                Vat_registration_status = VatRegistrationStatus_ToApiContractMap.Convert(legalEntityStateModel.VatRegistrationStatus),
                Country_of_authority = legalEntityStateModel.CountryOfAuthority,
                Company_registration = legalEntityStateModel.CompanyRegistration,
                Operating_as = legalEntityStateModel.TradingName, // TODO: confirm if this is correct mapping as there is also TradingName property in legal entity model
                Maximum_transaction_value = legalEntityStateModel.MaximumTransactionValue,
                Merchant_category_code = legalEntityStateModel.MerchantCategoryCode,
                Standard_industry_classification = legalEntityStateModel.StandardIndustryClassification,
                Turnover_per_annum = legalEntityStateModel.TurnoverPerAnnum,
                Card_turnover_per_annum = legalEntityStateModel.CardTurnoverPerAnnum,
                Trading_name = legalEntityStateModel.TradingName,
                Trading_industry_classification = legalEntityStateModel.TradingIndustryClassification,
                Business_identification = legalEntityStateModel.BusinessIdentification,
                Registered_addresses = RegisteredAddresses_ToApiContractMap.Convert(legalEntityStateModel.RegisteredAddresses),
                Business_contacts = BusinessContacts_ToApiContractMap.Convert(legalEntityStateModel.BusinessContacts),
                End_of_business_relationship = EndOfBusinessRelationship_ToApiContractMap.Convert(legalEntityStateModel.EndOfBusinessRelationship),
                Goods_ownership = GoodsOwnership_ToApiContractMap.Convert(legalEntityStateModel.GoodsOwnership),
                Legal_entity_status = LegalEntityStatus_ToApiContractMap.Convert(legalEntityStateModel.Status),
                Legal_entity_with_control = LegalEntitiesWithControl_ToApiContractMap.Convert(legalEntityStateModel.LegalEntitiesWithControl),
                Persons_with_control = PersonsWithControl_ToApiContractMap.Convert(legalEntityStateModel.PersonsWithControl),
                Partners_with_interest = PartnersWithInterest_ToApiContractMap.Convert(legalEntityStateModel.PartnersWithInterest)
            };
            if (legalEntityStateModel.MetaData != null)
            {
                var metaData = new ApiContract.MetaData();
                foreach (var data in legalEntityStateModel.MetaData)
                {
                    metaData.Add(data.Key, data.Value);
                }
                responseLegalEntity.Meta_data = metaData;
            }
            if (legalEntityStateModel.Labels != null)
            {
                var labels = new ApiContract.Labels();
                foreach (var label in legalEntityStateModel.Labels)
                {
                    labels.Add(label);
                }
                responseLegalEntity.Labels = labels;
            }
            if (legalEntityStateModel.SystemData != null)
            {
                var systemData = new ApiContract.SystemData();
                foreach (var data in legalEntityStateModel.SystemData)
                {
                    systemData.Add(data.Key, data.Value);
                }
                responseLegalEntity.System_data = systemData;
            }
            return responseLegalEntity;
        }
    }

    public class PersonsWithControl_ToApiContractMap
    {
        public static ApiContract.PersonWithControl[] Convert(PersonWithControl[] personsWithControl)
        {
            if (personsWithControl == null)
            {
                return null;
            }
            return personsWithControl.Select(pwc => new ApiContract.PersonWithControl
            {

            }).ToArray();
        }
    }

    public class PartnersWithInterest_ToApiContractMap
    {
        public static ApiContract.PartnerWithInterest[] Convert(PartnersWithInterest[] partnersWithInterest)
        {
            if (partnersWithInterest == null)
            {
                return null;
            }
            return partnersWithInterest.Select(pwi => new ApiContract.PartnerWithInterest
            {
                Attribution_type = Attribution_ToApiContractMap.Convert(pwi.Attributions),
                Legal_entity_id = pwi.LegalEntityId,

            }).ToArray();
        }
    }

    public class Attribution_ToApiContractMap
    {
        public static ApiContract.AttributionType[] Convert(Attribution[] attributions)
        {
            if (attributions == null)
            {
                return null;
            }

            return attributions.Select(a => a switch
            {
                Attribution.Introducer => ApiContract.AttributionType.Introducer,
                Attribution.Affiliate => ApiContract.AttributionType.Affiliate,
                Attribution.Reseller => ApiContract.AttributionType.Reseller,
                _ => ApiContract.AttributionType.Other
            }).ToArray();
        }

    }

    public class LegalEntitiesWithControl_ToApiContractMap
    {
        public static ApiContract.LegalEntityWithControl[] Convert(LegalEntityWithControl[] legalEntityWithControls)
        {
            if (legalEntityWithControls == null)
            {
                return null;
            }
            return legalEntityWithControls.Select(lec => new ApiContract.LegalEntityWithControl
            {
                Control_types = lec.ControlTypes.Select(ct => ControlType_ToApiContractMap.Convert(ct)).ToArray(),
                Legal_entity_id = lec.LegalEntityId,
                Date_from = DateTime.MinValue,
                Date_to = DateTime.MaxValue,
                Ownership_percentage = 10
            }).ToArray();
        }
    }

    public class ControlType_ToApiContractMap
    {
        public static ApiContract.LegalEntityControlType Convert(ControlType controlType)
        {
            return controlType switch
            {
                ControlType.Shareholder => ApiContract.LegalEntityControlType.Shareholder,
                ControlType.ParentCompany => ApiContract.LegalEntityControlType.Shareholder,
                _ => throw new ArgumentOutOfRangeException(nameof(controlType), $"Not expected control type value: {controlType}")
            };
        }
    }

    public class LegalEntityStatus_ToApiContractMap
    {
        public static ApiContract.LegalEntityStatus Convert(LegalEntityStatus legalEntityStatus)
        {
            return legalEntityStatus switch
            {
                LegalEntityStatus.Active => ApiContract.LegalEntityStatus.Active,
                LegalEntityStatus.Inactive => ApiContract.LegalEntityStatus.Inactive,
                LegalEntityStatus.Suspended => ApiContract.LegalEntityStatus.Terminated,
                _ => throw new ArgumentOutOfRangeException(nameof(legalEntityStatus), $"Not expected legal entity status value: {legalEntityStatus}")
            };
        }
    }

    public class GoodsOwnership_ToApiContractMap
    {
        public static ApiContract.GoodsOwnership Convert(GoodsOwnership goodsOwnership)
        {
            return goodsOwnership switch
            {
                GoodsOwnership.Owned => ApiContract.GoodsOwnership.Owned,
                GoodsOwnership.Leased => ApiContract.GoodsOwnership.Leased,
                GoodsOwnership.Other => ApiContract.GoodsOwnership.Other,
                GoodsOwnership.ThirdParty => ApiContract.GoodsOwnership.ThirdParty,
                _ => throw new ArgumentOutOfRangeException(nameof(goodsOwnership), $"Not expected goods ownership value: {goodsOwnership}")
            };
        }
    }

    public class EndOfBusinessRelationship_ToApiContractMap
    {
        public static ApiContract.BusinessRelationship Convert(EndOfBusinessRelationship endOfBusinessRelationship)
        {
            if (endOfBusinessRelationship == null)
            {
                return null;
            }
            return new ApiContract.BusinessRelationship
            {
                End_date = endOfBusinessRelationship.EndDate.ToShortDateString(),
                Reason = endOfBusinessRelationship.Reason
            };
        }
    }

    public class BusinessContacts_ToApiContractMap
    {
        public static ApiContract.BusinessContact[] Convert(BusinessContact[] businessContacts)
        {
            if (businessContacts == null)
            {
                return null;
            }
            return businessContacts.Select(bc => new ApiContract.BusinessContact
            {
                Contact_type = ContactType_ToApiContractMap.Convert(bc.ContactType),
                Contact_id = bc.ContactId
            }).ToArray();
        }
    }

    public class ContactType_ToApiContractMap
    {
        public static ApiContract.BusinessContactType Convert(ContactType contactType)
        {
            return contactType switch
            {
                ContactType.Financial => ApiContract.BusinessContactType.Financial,
                ContactType.Technical => ApiContract.BusinessContactType.Financial,
                ContactType.Developer => ApiContract.BusinessContactType.Developer,
                ContactType.User => ApiContract.BusinessContactType.User,
                _ => throw new ArgumentOutOfRangeException(nameof(contactType), $"Not expected contact type value: {contactType}")
            };
        }
    }

    public class RegisteredAddresses_ToApiContractMap
    {
        public static ApiContract.RegisteredAddress[] Convert(RegisteredAddress[] registeredAddresses)
        {
            if (registeredAddresses == null)
            {
                return null;
            }
            return registeredAddresses.Select(ra => new ApiContract.RegisteredAddress
            {
                Current = ra.Current,
                Date_from = ra.DateFrom,
                Date_to = ra.DateTo,
                Address = Address_ToApiContractMap.Convert(ra.Address)
            }).ToArray();
        }
    }

    public class Contact_ToApiContractMap
    {
        public static ApiContract.Contact Convert(Contact contactStateModel)
        {
            if (contactStateModel == null) { return null; }

            var responseContact = new ApiContract.Contact()
            {
                Name = contactStateModel.Name,
                Email = contactStateModel.Email,
                Alt_telephone = contactStateModel.AltTelephone,
                Alt_telephone_code = contactStateModel.AltTelephoneCode,
                Telephone = contactStateModel.Telephone,
                Telephone_code = contactStateModel.TelephoneCode,
                Postal_address = Address_ToApiContractMap.Convert(contactStateModel.PostalAddress),
            };

            if (contactStateModel.Labels != null)
            {
                var labels = new ApiContract.Labels();
                foreach (var label in contactStateModel.Labels)
                {
                    labels.Add(label);
                }
                responseContact.Labels = labels;
            }
            if (contactStateModel.MetaData != null)
            {
                var metaData = new ApiContract.MetaData();
                foreach (var data in contactStateModel.MetaData)
                {
                    metaData.Add(data.Key, data.Value);
                }
                responseContact.Meta_data = metaData;
            }
            if (contactStateModel.SystemData != null)
            {
                var systemData = new ApiContract.SystemData();
                foreach (var data in contactStateModel.SystemData)
                {
                    systemData.Add(data.Key, data.Value);
                }
                responseContact.System_data = systemData;
            }
            return responseContact;
        }
    }

    public class Address_ToApiContractMap
    {
        public static ApiContract.Address Convert(Address addressStateModel)
        {
            if (addressStateModel == null)
            {
                return null;
            }

            return new ApiContract.Address()
            {
                Line1 = addressStateModel.Line1,
                Line2 = addressStateModel.Line2,
                Line3 = addressStateModel.Line3,
                Region = addressStateModel.Region,
                Locality = addressStateModel.Locality,
                Code = addressStateModel.Code,
                Country = addressStateModel.Country,
                City = addressStateModel.City
            };
        }
    }

    public class MessageEnvelop_ToEntityResponse_BillingGroup
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

    public class FeedbackType_ToApiEntityStateKindMap
    {
        public static ApiContract.EntityStateResultKind Convert(FeedbackType feedbackType)
        {
            return feedbackType switch
            {
                FeedbackType.DocumentRequired => ApiContract.EntityStateResultKind.DocumentRequired,
                FeedbackType.WaitingForExternalRiskChecks => ApiContract.EntityStateResultKind.WaitingForExternalRiskChecks,
                FeedbackType.LegalEntityMissing => ApiContract.EntityStateResultKind.LegalEntityMissing,
                FeedbackType.WaitingForProductSelection => ApiContract.EntityStateResultKind.WaitingForProductSelection,
                FeedbackType.MissingRequiredInformation => ApiContract.EntityStateResultKind.MissingRequiredInformation,
                FeedbackType.InternalError => ApiContract.EntityStateResultKind.InternalError,
                FeedbackType.WaitingForContractSignatureOrAcceptance => ApiContract.EntityStateResultKind.WaitingForContractSignatureOrAcceptance,
                FeedbackType.UserActionRequired => ApiContract.EntityStateResultKind.UserActionRequired,
                FeedbackType.WaitingForLegalEntityApproval => ApiContract.EntityStateResultKind.WaitingForLegalEntityApproval,
                FeedbackType.WaitingForConfiguration => ApiContract.EntityStateResultKind.WaitingForConfiguration,

                _ => throw new ArgumentOutOfRangeException(nameof(feedbackType), $"Not expected feedback type value: {feedbackType}")
            };
        }
    }

    public class EntityState_ToApiStateMap
    {
        public static ApiContract.EntityState Convert(EntityState entityState)
        {
            return entityState switch
            {
                EntityState.NEW => ApiContract.EntityState.New,
                EntityState.EVALUATING => ApiContract.EntityState.Evaluating,
                EntityState.EVALUATION_RESTARTING => ApiContract.EntityState.EvaluationRestarting,
                EntityState.ATTENTION_REQUIRED => ApiContract.EntityState.AttentionRequired,
                EntityState.IN_REVIEW => ApiContract.EntityState.InReview,
                EntityState.IN_PROGRESS => ApiContract.EntityState.InProgress,
                EntityState.SYNCHRONISED => ApiContract.EntityState.Synchronised,

                _ => throw new ArgumentOutOfRangeException(nameof(entityState), $"Not expected entity state value: {entityState}")
            };
        }
    }
}
