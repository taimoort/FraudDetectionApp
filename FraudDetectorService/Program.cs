
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using FraudDetectorService;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Microsoft.AspNetCore.Http.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);
var esUri = builder.Configuration["Elasticsearch:Uri"];
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(esUri))
    {
        IndexFormat = "fraud-{0:yyyy.MM.dd}"
    })
    .CreateLogger();
// read bootstrap from config/env
//var kafkaBootstrap = builder.Configuration["Kafka:BootstrapServers"];
builder.Services.AddSingleton<KafkaConsumerService>();
builder.Services.AddHostedService<KafkaConsumerService>();
// builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
// {
//     var cfg = new ProducerConfig { BootstrapServers = kafkaBootstrap };
//     return new ProducerBuilder<Null, string>(cfg).Build();
// });

builder.Services.AddSerilog();
builder.Services.AddDbContext<FraudContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("FraudDatabase")));
//builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

// (2)  Minimal API route — latest 50 fraud rows
app.MapGet("/api/alerts", async (FraudContext db) =>
    await db.FlaggedTransactions
            .OrderByDescending(f => f.FlaggedAt)
            .Take(50)
            .ToListAsync());

app.Run();
