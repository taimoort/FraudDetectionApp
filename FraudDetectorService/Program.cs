using FraudDetectorService;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Npgsql.EntityFrameworkCore.PostgreSQL;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        IndexFormat = "fraud-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog();
builder.Services.AddDbContext<FraudContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("FraudDatabase")));
builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddHostedService<Worker>();



var host = builder.Build();
host.Run();
