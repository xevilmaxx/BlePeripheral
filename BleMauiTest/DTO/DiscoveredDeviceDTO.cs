using Plugin.BLE.Abstractions.Contracts;

namespace BleMauiTest.DTO
{
    public class DiscoveredDeviceDTO
    {

        public IDevice Device { get; set; }
        public DateTime LastChange { get; set; }

        public DiscoveredDeviceDTO(IDevice Device)
        {
            this.Device = Device;
            LastChange = DateTime.Now;
        }

    }
}
