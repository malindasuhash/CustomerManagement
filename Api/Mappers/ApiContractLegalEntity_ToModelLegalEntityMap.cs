using StateManagment.Entity;

namespace Api.Mappers
{
    public class ApiContractLegalEntity_ToModelLegalEntityMap
    {
        public static LegalEntity Convert(ApiContract.CreateLegalEntity apiContractLegalEntity)
        {
            if (apiContractLegalEntity == null)
            {
                return null;
            }

            var modelLegalEntity = new LegalEntity()
            {
                Name = apiContractLegalEntity.Name,
                BusinessEmail = apiContractLegalEntity.Business_email,
                BusinessType = BusinessType_ToModelBusinessTypeMap.Convert(apiContractLegalEntity.Business_type),
                DateBusinessStarted = DateTime.Parse(apiContractLegalEntity.Date_business_created),
                DateTradingStarted = DateTime.Parse(apiContractLegalEntity.Date_trading_started),
                CharityRegistration = apiContractLegalEntity.Charity_registration,
                VatRegistration = apiContractLegalEntity.Vat_registration,
                VatRegistrationStatus = VatRegistrationStatus_ToModelVatRegistrationStatusMap.Convert(apiContractLegalEntity.Vat_registration_status),
                CountryOfAuthority = apiContractLegalEntity.Country_of_authority,
                CompanyRegistration = apiContractLegalEntity.Company_registration,
                TradingName = apiContractLegalEntity.Operating_as,
                MaximumTransactionValue = apiContractLegalEntity.Maximum_transaction_value,
                MerchantCategoryCode = apiContractLegalEntity.Merchant_category_code,
                StandardIndustryClassification = apiContractLegalEntity.Standard_industry_classification,
                TurnoverPerAnnum = apiContractLegalEntity.Turnover_per_annum,
                CardTurnoverPerAnnum = apiContractLegalEntity.Card_turnover_per_annum,
                TradingIndustryClassification = apiContractLegalEntity.Trading_industry_classification,
                BusinessIdentification = apiContractLegalEntity.Business_identification,
                OperatingAs = apiContractLegalEntity.Operating_as,
                RegisteredAddresses = RegisteredAddresses_ToModelRegisteredAddressesMap.Convert(apiContractLegalEntity.Registered_addresses),
                BusinessContacts = BusinessContacts_ToModelBusinessContactsMap.Convert(apiContractLegalEntity.Business_contacts),
                //EndOfBusinessRelationship = EndOfBusinessRelationship_ToModelEndOfBusinessRelationshipMap.Convert(apiContractLegalEntity.End_of_business_relationship),
                GoodsOwnership = GoodsOwnership_ToModelGoodsOwnershipMap.Convert(apiContractLegalEntity.Goods_ownership),
                LegalEntitiesWithControl = LegalEntitiesWithControl_ToModelLegalEntitiesWithControlMap.Convert(apiContractLegalEntity.Legal_entity_with_control),
                PartnersWithInterest = PartnersWithInterest_ToModelPartnersWithInterestMap.Convert(apiContractLegalEntity.Partners_with_interest),
                PersonsWithControl = PersonsWithControl_ToModelPersonsWithControlMap.Convert(apiContractLegalEntity.Persons_with_control),
                // Status = LegalEntityStatus_ToModelLegalEntityStatusMap.Convert(apiContractLegalEntity.sta)
            };

            if (apiContractLegalEntity.Meta_data != null)
            {
                var metaDataList = new List<MetaDataModel>();
                foreach (var data in apiContractLegalEntity.Meta_data)
                {
                    metaDataList.Add(new MetaDataModel { Key = data.Key, Value = data.Value });
                }
                modelLegalEntity.MetaData = [.. metaDataList];
            }
            if (apiContractLegalEntity.Labels != null)
            {
                modelLegalEntity.Labels = [.. apiContractLegalEntity.Labels];
            }

            if (apiContractLegalEntity.System_data != null)
            {
                var systemDataList = new List<SystemDataModel>();
                foreach (var data in apiContractLegalEntity.System_data)
                {
                    systemDataList.Add(new SystemDataModel { Key = data.Key, Value = data.Value });
                }
                modelLegalEntity.SystemData = [.. systemDataList];
            }
            
            return modelLegalEntity;
        }
    }
    }
