using StateManagment.Entity;

namespace StateManagment.Models
{
    public class CustomerProfile
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public string[] Labels { get; set; }
        public MetaDataModel[] MetaData { get; set; }
        public SystemDataModel[] SystemData { get; set; }
    }
}
