using Confluent.Kafka;
using System;
using System.IO;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Avro;
using Avro.Generic;
using Shared.Contracts;
using System.Text.Json;
using FraudDetectorService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using Confluent.Kafka.SyncOverAsync;

public class KafkaConsumerService : BackgroundService
{
    private readonly ConsumerConfig _config;
    private readonly IServiceProvider _services;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly string _environmentName;
    private readonly CachedSchemaRegistryClient _schemaRegistry;
    private readonly AvroDeserializer<Transaction> _avroDeserializer;
    private readonly AvroSerializer<Transaction> _avroSerializer;
    private readonly IProducer<string, Transaction> _producer;

    public KafkaConsumerService(IConfiguration cfg, IServiceProvider services, ILogger<KafkaConsumerService> logger, IHostEnvironment env)
    {
        _services = services;
        _logger = logger;
        _environmentName = env.EnvironmentName;
        var producerConfig = new ProducerConfig { BootstrapServers = cfg["Kafka:BootstrapServers"] };
        var schemaConfig = new SchemaRegistryConfig { Url = cfg["SchemaRegistry:Url"] };
        // client to talk to Schema Registry
        _schemaRegistry = new CachedSchemaRegistryClient(schemaConfig);
        // build the Kafka producer
        _avroSerializer = new AvroSerializer<Transaction>(_schemaRegistry);
        _avroDeserializer = new AvroDeserializer<Transaction>(_schemaRegistry);
        _producer = new ProducerBuilder<string, Transaction>(producerConfig)
            .SetValueSerializer(_avroSerializer.AsSyncOverAsync())
            .Build();
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
            using var consumer = new ConsumerBuilder<string, Transaction>(_config)
            .SetKeyDeserializer(Deserializers.Utf8)
            .SetValueDeserializer(_avroDeserializer.AsSyncOverAsync())
            .Build();
            consumer.Subscribe("transactions");
            _logger.LogInformation("Kafka consumer subscribed to ‘transactions’ topic");
            /* using var adminClient = new AdminClientBuilder(
                            new AdminClientConfig { BootstrapServers = _config.BootstrapServers }
                        ).Build(); */
            while (!stoppingToken.IsCancellationRequested)
            {
                /* var meta = adminClient.GetMetadata("transactions", TimeSpan.FromSeconds(10)); */
                var result = consumer.Consume(stoppingToken);
                var record = result.Message.Value;
                bool isFraud = record.Amount > 10000;
                double score = isFraud ? 0.9f : 0f;      // or your real model
                string reason = isFraud ? "Amount threshold" : "";
                // Basic fraud rule
                if (isFraud)
                {
                    _logger.LogWarning("Flagging tx {Id} for amount {Amt}", record.TransactionId, record.Amount);

                    // create a scope to resolve a new DbContext
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<FraudContext>();
                    db.FlaggedTransactions.Add(new FlaggedTransaction
                    {
                        TransactionId = Guid.Parse(record.TransactionId),
                        Amount = (decimal)record.Amount,
                        // convert the timestamp to UTC for PostgreSQL timestamptz
                        Timestamp = record.Timestamp,
                        FlaggedAt = DateTime.UtcNow
                    });
                    db.SaveChanges();
                }
                // now emit enriched event:
                var enrichedRecord = new Transaction
                {
                    TransactionId = record.TransactionId,
                    Amount = record.Amount,
                    Timestamp = record.Timestamp,
                    IsFraud = isFraud,
                    FraudScore = score,
                    FraudReason = reason,
                    SourceService = "fraud-detector",
                    Environment = _environmentName
                };
                _producer.Produce("transactions_enriched", new Message<string, Transaction> { Key = record.TransactionId, Value = enrichedRecord });
                _producer.Flush(TimeSpan.FromSeconds(5));
            }
        }, stoppingToken);
}