using StateManagment.Models;

namespace StateManagment.Entity
{
    public interface IEntity
    {   
    }

    public interface ILegalEntityAttached
    {
        string LegalEntityId { get; set; }
    }
}
