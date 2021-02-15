using App.Metrics;
using App.Metrics.Counter;

namespace CustomResourceDescriptorController.metrics
{
    public static class Counters
    {
        public static CounterOptions EventReceived = new CounterOptions
        {
            Name = "Event Received",
            MeasurementUnit = Unit.Calls,
        };

    }
}