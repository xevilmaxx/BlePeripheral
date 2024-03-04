using System.Collections.Generic;

namespace LibBlePeripheral.LinuxBluezServer.Core
{
    internal interface IObjectManagerProperties
    {
        IDictionary<string, IDictionary<string, object>> GetProperties();
    }
}