using System.Collections.Generic;
using Tmds.DBus;

namespace LibBlePeripheral.LinuxBluezServer.Core
{
    public static class BlueZManager
    {
        internal static bool IsMatch(string interfaceName, ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfaces, IDBusObject rootObject)
        {
            return IsMatch(interfaceName, objectPath, interfaces.Keys, rootObject);
        }

        internal static bool IsMatch(string interfaceName, ObjectPath objectPath, ICollection<string> interfaces, IDBusObject rootObject)
        {
            if (rootObject != null && !objectPath.ToString().StartsWith($"{rootObject.ObjectPath}/"))
            {
                return false;
            }
            return interfaces.Contains(interfaceName);
        }
    }

}
