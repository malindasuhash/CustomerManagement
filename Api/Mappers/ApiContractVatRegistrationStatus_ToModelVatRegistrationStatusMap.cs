
using StateManagment.Entity;

namespace Api.Mappers
{
    internal class ApiContractVatRegistrationStatus_ToModelVatRegistrationStatusMap
    {
        internal static StateManagment.Entity.VatRegistrationStatus Convert(ApiContract.VatRegistrationStatus? vat_registration_status)
        {
            if (vat_registration_status == null)
            {
                return VatRegistrationStatus.None;
            }

            switch (vat_registration_status)
            {
                case ApiContract.VatRegistrationStatus.Registered:
                    return VatRegistrationStatus.Registered;
                case ApiContract.VatRegistrationStatus.NotRegistered:
                    return VatRegistrationStatus.NotRegistered;
                case ApiContract.VatRegistrationStatus.RegistrationPending:
                    return VatRegistrationStatus.Pending;
            }
            
            return VatRegistrationStatus.Other;
        }
    }
}