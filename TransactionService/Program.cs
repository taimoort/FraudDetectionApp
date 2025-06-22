using Serilog;
using Serilog.Sinks.Elasticsearch;
using TransactionService;
var builder = WebApplication.CreateBuilder(args);
var esUri = builder.Configuration["Elasticsearch:Uri"];
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(esUri))
    {
        IndexFormat = "KafkaProducerService-{0:yyyy.MM.dd}"
    })
    .CreateLogger();
;
builder.Services.AddSerilog();
// Add services to the container.
builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddControllers();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map controllers
app.MapControllers();

app.Run();
