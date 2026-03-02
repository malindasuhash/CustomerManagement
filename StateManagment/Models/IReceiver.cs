namespace StateManagment.Models
{
    public interface IReceiver
    {
        void ReceiveAsync();
        void StopAync();
        void StartAsync();
    }
}
