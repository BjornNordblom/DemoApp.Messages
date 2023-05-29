using System.Text.Json;
using DemoApp.Messages;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Serilog;

var configuration = new LoggerConfiguration().MinimumLevel
    .Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override(
        "Microsoft.EntityFrameworkCore",
        Serilog.Events.LogEventLevel.Information
    )
    .MinimumLevel.Override(
        "Microsoft.EntityFrameworkCore.Database.Command",
        Serilog.Events.LogEventLevel.Information
    )
    .MinimumLevel.Override(
        "Microsoft.EntityFrameworkCore.Infrastructure",
        Serilog.Events.LogEventLevel.Information
    )
    .WriteTo.Console()
    .Enrich.FromLogContext();
var logger = configuration.CreateLogger();
Log.Logger = logger;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(logger);
builder.Services.AddLogging();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>();
builder.Services.AutoRegisterHandlersFromAssemblyOf<Program>();

builder.Services.AddRebus(
    (configure) =>
    {
        return configure
            .Logging(l => l.Serilog(logger))
            .Transport(t => t.UseRabbitMq("amqp://localhost", "DemoApp"))
            .Routing(r => r.TypeBased().MapAssemblyOf<Program>("DemoApp"))
            .Options(o => o.SetNumberOfWorkers(1));
    },
    onCreated: async bus =>
    {
        await bus.Subscribe<Outbox>();
    }
);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseSerilogRequestLogging();

// Get data context from app
// using the local scope
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    // Delete and create database
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

app.MapGet(
    "/",
    (ILogger<Program> logger, IBus bus, DataContext context) =>
    {
        logger.LogInformation("Sending message");
        // Push message
        var myContent = new MyContent { Name = "Test", Value = "Test" };
        var outbox = new Outbox
        {
            Category = "MyContentCreated",
            Type = "MyContent",
            Content = JsonSerializer.Serialize(myContent)
        };
        var origOutboxId = outbox.Id;
        context.Outbox.Add(outbox);
        context.SaveChanges();

        // Fetch message
        var fetchedOutbox = context.Outbox.Find(origOutboxId);
        if (fetchedOutbox == null)
        {
            logger.LogError("Outbox with {id} not found", origOutboxId);
            return;
        }

        bus.Publish(fetchedOutbox);
        logger.LogInformation("Message {id} sent", fetchedOutbox.Id);
    }
);

app.Run();
