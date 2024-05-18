using MassTransit;
using Trumpee.MassTransit.Messages.Notifications;

namespace Prioritize;

public class PrioritizationConsumer : IConsumer<Notification>
{
    public async Task Consume(ConsumeContext<Notification> context)
    {
        var message = context.Message;
        Console.WriteLine(
            $"Received message: {message.Timestamp}-{message.Priority}");

        // TODO: Add prioritization logic here...
        var queueName = message.Priority switch
        {
            Priority.Low => "delivery-low",
            Priority.Medium => "delivery-medium",
            Priority.High => "delivery-high",
            Priority.Critical => "delivery-critical",
            _ => "no-queue" // TODO: write to DLQ
        };

        await context.Send(new Uri($"queue:{queueName}"), context.Message);
    }
}