using BeaconLab.Contracts;

namespace BeaconLab.Producer;

// Stands in for: a sensor or forwarder publishing to Event Hubs.
// Same: partition key is DeviceName. Only at swap-in: the connection.
// Not this: a Function.
public interface IEventPublisher
{
    Task PublishAsync(OutboundEvent outbound);
}
