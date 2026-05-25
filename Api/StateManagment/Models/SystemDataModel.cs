

namespace StateManagment.Models
{
    internal class SystemDataModel : Entity.SystemDataModel
    {
        public IDictionary<string, string> Data { get; set; }
    }
}