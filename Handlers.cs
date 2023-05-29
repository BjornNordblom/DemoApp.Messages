using System.Text.Json;
using DemoApp.Messages;
using Rebus.Handlers;

namespace DemoApp.Handlers;

public class OutboxEventHandler : IHandleMessages<Outbox>
{
    private readonly ILogger<OutboxEventHandler> _logger;
    private readonly DataContext _context;

    public OutboxEventHandler(ILogger<OutboxEventHandler> logger, DataContext context)
    {
        _logger = logger;
        _logger.LogInformation("TestMessageEventHandler created");
        _context = context;
    }

    public async Task Handle(Outbox message)
    {
        _logger.LogInformation("TestMessageEventHandler triggered");
        await Task.Delay(1000);
        var outbox = await _context.Outbox.FindAsync(message.Id);
        if (outbox == null)
        {
            _logger.LogError("Outbox with {id} not found", message.Id);
            return;
        }

        var type = Type.GetType(outbox.Type);
        if (type == null)
        {
            _logger.LogError("Type {outboxType} not found", outbox.Type);
            return;
        }
        var content = JsonSerializer.Deserialize(outbox.Content, type);
        _logger.LogInformation("Message content: {content}", content);
        outbox.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("TestMessageEventHandler processed");
    }
}
