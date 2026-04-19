using StateManagment.Entity;

namespace Contact.Orchestration.Svc.Model
{

    // This class is needed for deserialisation purposes.
    public class ContactRequestData : RequestData
    {
        public StateManagment.Entity.Contact Submitted { get; set; }
        public StateManagment.Entity.Contact Applied { get; set; }
    }
}
