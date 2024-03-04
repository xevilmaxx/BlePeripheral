using LibBlePeripheral.LinuxBluezServer.Core;
using LibBlePeripheral.LinuxBluezServer.Gatt.BlueZModel;
using System.Threading.Tasks;

namespace LibBlePeripheral.LinuxBluezServer.Gatt
{
    public abstract class ICharacteristicSource
    {
        public PropertiesBase<GattCharacteristic1Properties> Properties;
        public abstract Task WriteValueAsync(byte[] value, bool response, string objPath);
        public abstract Task<byte[]> ReadValueAsync(string objPath);
        public abstract Task StartNotifyAsync();
        public abstract Task StopNotifyAsync();
        public abstract Task ConfirmAsync();
    }
}
