using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

public class KafkaProducerService
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> logger)
    {
        var producerConfig = new ProducerConfig { BootstrapServers = config["Kafka:BootstrapServers"] };
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        _logger = logger;
    }

    public async Task SendMessageAsync(string topic, string message)
    {
        await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
        /* using var admin = new AdminClientBuilder(
        new AdminClientConfig { BootstrapServers = "localhost:9092" }
    ).Build();
        try
        {
            var meta = admin.GetMetadata("transactions", TimeSpan.FromSeconds(10));
            // breakpoint here to inspect meta…
        }
        catch (KafkaException ex)
        {
            _logger.LogError(ex, "Failed to fetch Kafka metadata");
        } */
    }
}
