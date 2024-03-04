
using LibBlePeripheral.LinuxBluezServer.Constants;
using System.Threading.Tasks;
using Tmds.DBus;

namespace LibBlePeripheral.LinuxBluezServer.Core
{
    public static class AdapterManager
    {
        public static IAdapter1 CurrentAdapter { get; private set; } = null;
        public static IAdapter1 GetAdapter(ServerContext context)
        {
            CurrentAdapter = context.Connection.CreateProxy<IAdapter1>(
               BlueZConstants.BASE_PATH,
                BlueZConstants.ADAPTER_PATH
            );
            return CurrentAdapter;
        }
        public static async Task<bool> SetAdapterPowerOn(ServerContext context)
        {
            CurrentAdapter ??= GetAdapter(context);
            await CurrentAdapter.SetPoweredAsync(true);
            return await CurrentAdapter.GetPoweredAsync();
        }
        public static async Task SetAdapterPowerOff(ServerContext context)
        {
            CurrentAdapter ??= GetAdapter(context);
            await CurrentAdapter.SetPoweredAsync(false);
        }

        public static async Task RemoveDeviceAsync(ServerContext context, ObjectPath path)
        {
            CurrentAdapter ??= GetAdapter(context);
            await CurrentAdapter.RemoveDeviceAsync(path);
        }
    }

}
