using System.Runtime.InteropServices;

namespace LibBlePeripheral.LinuxBluezServer.Utilities
{
    internal static class OsUtility
    {
        public static bool IsOperatingSystem(OSPlatform os)
        {
            return RuntimeInformation.IsOSPlatform(os);
        }
    }
}
