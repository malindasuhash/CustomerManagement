
namespace Api.Mappers
{
    internal class VatRegistrationStatus_ToModelVatRegistrationStatusMap
    {
        internal static StateManagment.Entity.VatRegistrationStatus Convert(ApiContract.VatRegistrationStatus? vat_registration_status)
        {
            if (!vat_registration_status.HasValue)
            {
                return StateManagment.Entity.VatRegistrationStatus.None;
            }

            switch (vat_registration_status.Value)
            {
                case ApiContract.VatRegistrationStatus.Registered:
                    return StateManagment.Entity.VatRegistrationStatus.Registered;
                case ApiContract.VatRegistrationStatus.NotRegistered:
                    return StateManagment.Entity.VatRegistrationStatus.NotRegistered;
                case ApiContract.VatRegistrationStatus.RegistrationPending:
                    return StateManagment.Entity.VatRegistrationStatus.Pending;
                default:
                    return StateManagment.Entity.VatRegistrationStatus.None;
            }
        }
    }
}