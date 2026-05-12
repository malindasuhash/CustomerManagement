using ContractFirstDesign;

namespace ContractFirstDesign.Examples;

public class DtoUsageExample
{
    public CreateLegalEntity BuildNewLegalEntity()
    {
        return new CreateLegalEntity
        {
            Name = "Acme Ltd",
            Business_email = "info@acme.com",
            Business_type = BusinessType.Standalone_FCA_Regulated,
            Company_registration = "12345678",
            Trading_name = "Acme Trading",
            Merchant_category_code = "5411",
            Standard_industry_classification = "4711",
            Turnover_per_annum = 500000,
            Card_turnover_per_annum = 200000,
            Maximum_transaction_value = 5000,
            Vat_registration_status = VatRegistrationStatus.Registered,
            Vat_registration = "GB123456789",
            Date_business_created = "2020-06-15",
            Date_trading_started = "2020-07-01",
            Persons_with_control =
            [
                new PersonWithControl
                {
                    Person = new Person
                    {
                        Title = "Mr",
                        First_name = "John",
                        Last_name = "Smith",
                        Date_of_birth = "1985-03-20",
                        Nationality = "GB",
                        Addresses = new List<RegisteredAddress>
                        {
                            new RegisteredAddress
                {
                    Current = true,
                    Date_from = DateTime.Parse("2020-06-15"),
                    Address = new Address
                    {
                        Name = "Acme House",
                        Line1 = "1 Business Park",
                        Locality = "Manchester",
                        Region = "Greater Manchester",
                        Code = "M1 1AA",
                        Country = "GB"
                    }
                },
                             new RegisteredAddress
                {
                    Current = true,
                    Date_from = DateTime.Parse("2020-06-15"),
                    Address = new Address
                    {
                        Name = "Acme House",
                        Line1 = "1 Business Park",
                        Locality = "Manchester",
                        Region = "Greater Manchester",
                        Code = "M1 1AA",
                        Country = "GB"
                    }
                }
                        }.ToArray()
                    },
                    Control_types = [PersonControlType.Director, PersonControlType.Shareholder]
                }
            ],
            Registered_addresses =
            [
                new RegisteredAddress
                {
                    Current = true,
                    Date_from = DateTime.Parse("2020-06-15"),
                    Address = new Address
                    {
                        Name = "Acme House",
                        Line1 = "1 Business Park",
                        Locality = "Manchester",
                        Region = "Greater Manchester",
                        Code = "M1 1AA",
                        Country = "GB"
                    }
                }
            ]
        };
    }
}