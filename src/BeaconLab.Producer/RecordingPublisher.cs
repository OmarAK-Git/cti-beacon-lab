using BeaconLab.Contracts;

namespace BeaconLab.Producer;

public sealed class RecordingPublisher : IEventPublisher
{
    public List<OutboundEvent> Sent { get; } = new();

    public Task PublishAsync(OutboundEvent outbound)
    {
        Sent.Add(outbound);
        return Task.CompletedTask;
    }
}
