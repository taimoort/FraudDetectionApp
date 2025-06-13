using Confluent.Kafka;
using Shared.Contracts;
using System.Text.Json;
using FraudDetectorService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

public class KafkaConsumerService : BackgroundService
{
    private readonly ConsumerConfig _config;
    private readonly IServiceProvider _services;
    private readonly ILogger<KafkaConsumerService> _logger;

    public KafkaConsumerService(IConfiguration cfg, IServiceProvider services, ILogger<KafkaConsumerService> logger)
    {
        _services = services;
        _logger = logger;
        _config = new ConsumerConfig
        {
            BootstrapServers = cfg["Kafka:BootstrapServers"],  // picks env/appsettings
            GroupId = "fraud-detector",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(() =>
        {
            using var consumer = new ConsumerBuilder<Null, string>(_config).Build();
            consumer.Subscribe("transactions");
            _logger.LogInformation("Kafka consumer subscribed to ‘transactions’ topic");
            /* using var adminClient = new AdminClientBuilder(
                            new AdminClientConfig { BootstrapServers = _config.BootstrapServers }
                        ).Build(); */
            while (!stoppingToken.IsCancellationRequested)
            {
                /* var meta = adminClient.GetMetadata("transactions", TimeSpan.FromSeconds(10)); */
                var result = consumer.Consume(stoppingToken);
                var tx = JsonSerializer.Deserialize<TransactionDto>(result.Message.Value);

                // Basic fraud rule
                if (tx.Amount > 10000m)
                {
                    _logger.LogWarning("Flagging tx {Id} for amount {Amt}", tx.TransactionId, tx.Amount);

                    // create a scope to resolve a new DbContext
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<FraudContext>();

                    var flagged = new FlaggedTransaction
                    {
                        TransactionId = tx.TransactionId,
                        Amount = tx.Amount,
                        Timestamp = tx.Timestamp,
                        FlaggedAt = DateTime.UtcNow
                    };

                    db.FlaggedTransactions.Add(flagged);
                    db.SaveChanges();
                }
            }
        }, stoppingToken);
}