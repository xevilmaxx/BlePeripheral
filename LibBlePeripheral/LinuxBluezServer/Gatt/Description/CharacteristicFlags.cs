using System;

namespace LibBlePeripheral.LinuxBluezServer.Gatt.Description
{
    [Flags]
    public enum CharacteristicFlags
    {
        None = 0,
        Read = 1,
        Write = 2,
        WritableAuxiliaries = 4,
        Notify = 8,
        Indicate = 16
    }

    public enum GattDescriptorFLags
    {
        read, write
    }
}
