using Confluent.Kafka;
using System.IO;
using Avro;
using Avro.Generic;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Shared.Contracts;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

public class KafkaProducerService
{
    //private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly CachedSchemaRegistryClient _schemaRegistry;
    private readonly AvroSerializer<Transaction> _avroSerializer;
    private readonly IProducer<Null, Transaction> _producer;
    private readonly Avro.Schema _recordSchema;

    public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> logger)
    {
        var producerConfig = new ProducerConfig { BootstrapServers = config["Kafka:BootstrapServers"] };

        // Schema Registry client
        var schemaConfig = new SchemaRegistryConfig { Url = config["SchemaRegistry:Url"] };
        _schemaRegistry = new CachedSchemaRegistryClient(schemaConfig);

        // Avro Serializer
        _avroSerializer = new AvroSerializer<Transaction>(_schemaRegistry);

        // Build Avro producer
        _producer = new ProducerBuilder<Null, Transaction>(producerConfig)
            .SetValueSerializer(_avroSerializer)
            .Build();

        _logger = logger;
    }

    public async Task SendMessageAsync(string topic, TransactionDto txDto)
    {
        //await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
        // Create GenericRecord from DTO
        var record = new Transaction
        {
            TransactionId = txDto.TransactionId.ToString(),
            Amount = (double)txDto.Amount,
            Timestamp = txDto.Timestamp,
        };
        // Produce Avro message
        await _producer.ProduceAsync(topic, new Message<Null, Transaction> { Value = record });
        _producer.Flush(TimeSpan.FromSeconds(5));
        // _logger.LogInformation("Produced Avro message to topic {Topic}: {TxId}", topic, txDto.TransactionId);

        // using var admin = new AdminClientBuilder(
        //     new AdminClientConfig { BootstrapServers = "localhost:9092" }
        // ).Build();
        // try
        // {
        //     var meta = admin.GetMetadata("transactions", TimeSpan.FromSeconds(10));
        //     // breakpoint here to inspect meta…
        // }
        // catch (KafkaException ex)
        // {
        //     _logger.LogError(ex, "Failed to fetch Kafka metadata");
        // }
    }
}
