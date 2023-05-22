using DemoApp.Messages;
using Rebus.Handlers;

namespace DemoApp.Handlers;

public class TestMessageEventHandler : IHandleMessages<TestMessage>
{
    private readonly ILogger<TestMessageEventHandler> _logger;

    public TestMessageEventHandler(ILogger<TestMessageEventHandler> logger)
    {
        _logger = logger;
        _logger.LogInformation("TestMessageEventHandler created");
    }

    public async Task Handle(TestMessage message)
    {
        _logger.LogInformation("TestMessageEventHandler triggered");
        // handle message in here
    }
}
