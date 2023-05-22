using DemoApp.Handlers;
using DemoApp.Messages;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
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
        await bus.Subscribe<TestMessage>();
    }
);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseSerilogRequestLogging();

app.MapGet(
    "/",
    (ILogger<TestMessage> logger, IBus bus) =>
    {
        logger.LogInformation("Sending message");
        bus.Publish(new TestMessage("Hello from Rebus!"));
        logger.LogInformation("Message sent");
    }
);

app.Run();
