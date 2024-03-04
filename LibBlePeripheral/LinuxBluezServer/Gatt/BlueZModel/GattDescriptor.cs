using LibBlePeripheral.LinuxBluezServer.Constants;
using LibBlePeripheral.LinuxBluezServer.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace LibBlePeripheral.LinuxBluezServer.Gatt.BlueZModel
{
    public class GattDescriptor
        : PropertiesBase<GattDescriptor1Properties>,
            IGattDescriptor1,
            IObjectManagerProperties
    {
        public GattDescriptor(
            ObjectPath objectPath,
            GattDescriptor1Properties gattDescriptor1Properties
        )
            : base(objectPath, gattDescriptor1Properties) { }

        public Task<byte[]> ReadValueAsync()
        {
            return Task.FromResult(Properties.Value);
        }

        public IDictionary<string, IDictionary<string, object>> GetProperties()
        {
            return new Dictionary<string, IDictionary<string, object>>()
            {
                {
                    BlueZConstants.GATT_DESCRIPTOR_PROP,
                    new Dictionary<string, object>
                    {
                        { "Characteristic", Properties.Characteristic },
                        { "UUID", Properties.UUID },
                        { "Flags", Properties.Flags }
                    }
                }
            };
        }
    }
}
