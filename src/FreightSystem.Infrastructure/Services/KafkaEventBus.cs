using FreightSystem.Application.Interfaces;

namespace FreightSystem.Infrastructure.Services
{
    public class KafkaEventBus : IEventBus
    {
        // Stubbed implementation; replace with Confluent.Kafka producer/consumer when available
        private static readonly Dictionary<string, List<Func<string, Task>>> _handlers = new();

        public Task PublishAsync(string topic, object @event)
        {
            if (_handlers.TryGetValue(topic, out var subs))
            {
                var payload = System.Text.Json.JsonSerializer.Serialize(@event);
                return Task.WhenAll(subs.Select(h => h(payload)));
            }
            return Task.CompletedTask;
        }

        public Task SubscribeAsync(string topic, Func<string, Task> handler)
        {
            if (!_handlers.ContainsKey(topic))
                _handlers[topic] = new List<Func<string, Task>>();

            _handlers[topic].Add(handler);
            return Task.CompletedTask;
        }
    }
}
