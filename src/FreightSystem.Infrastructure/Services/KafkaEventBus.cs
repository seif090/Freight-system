using Confluent.Kafka;
using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Settings;
using Microsoft.Extensions.Options;

namespace FreightSystem.Infrastructure.Services
{
    public class KafkaEventBus : IEventBus
    {
        private readonly IProducer<Null, string> _producer;
        private readonly KafkaSettings _settings;

        public KafkaEventBus(IOptions<KafkaSettings> options)
        {
            _settings = options.Value;

            var config = new ProducerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                ClientId = _settings.ClientId,
                SecurityProtocol = _settings.SecurityProtocol?.ToLower() == "sasl_ssl" ? SecurityProtocol.SaslSsl : SecurityProtocol.Plaintext,
                SaslMechanism = _settings.SaslMechanism?.ToUpper() == "PLAIN" ? SaslMechanism.Plain : SaslMechanism.Plain,
                SaslUsername = _settings.SaslUsername,
                SaslPassword = _settings.SaslPassword,
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageTimeoutMs = 30000
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task PublishAsync(string topic, object @event)
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(@event);
            try
            {
                var msg = new Message<Null, string> { Value = payload };
                var result = await _producer.ProduceAsync(topic, msg);
                _producer.Flush(TimeSpan.FromSeconds(5));
            }
            catch (ProduceException<Null, string> ex)
            {
                // fallback to debug logger in case Kafka is unreachable
                Console.Error.WriteLine($"Kafka produce failed: {ex.Error.Reason}");
            }
        }

        public Task SubscribeAsync(string topic, Func<string, Task> handler)
        {
            Task.Run(() => ConsumeLoop(topic, handler));
            return Task.CompletedTask;
        }

        private async Task ConsumeLoop(string topic, Func<string, Task> handler)
        {
            if (string.IsNullOrEmpty(_settings.BootstrapServers))
                return;

            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = $"freight-system-{topic}-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(topic);

            while (true)
            {
                try
                {
                    var result = consumer.Consume(CancellationToken.None);
                    if (result?.Message?.Value != null)
                    {
                        await handler(result.Message.Value);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Kafka consume error: {ex.Message}");
                }
            }
        }
    }
}
