namespace StateManagment.Models
{
    public interface ICustomerProfileRepository
    {
        Task<TaskOutcome> Create(CustomerProfile customerProfile);
        Task<CustomerProfile?> Read(string customerId);
        Task<TaskOutcome> Update(CustomerProfile customerProfile);
        Task<TaskOutcome> Delete(string customerId);
    }
}
