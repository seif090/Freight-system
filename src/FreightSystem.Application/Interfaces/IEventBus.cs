namespace FreightSystem.Application.Interfaces
{
    public interface IEventBus
    {
        Task PublishAsync(string topic, object @event);
        Task SubscribeAsync(string topic, Func<string, Task> handler);
    }
}
