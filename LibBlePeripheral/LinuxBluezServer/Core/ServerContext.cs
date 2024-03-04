using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace LibBlePeripheral.LinuxBluezServer.Core
{
    public class ServerContext : IDisposable
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public ServerContext()
        {
            if (OperatingSystem.IsLinux() == true)
            {
                Connection = new Connection(Address.System);
            }
            else
            {
                log.Debug("NOT LINUX!");
            }
        }

        public async Task Connect()
        {
            if (OperatingSystem.IsLinux() == true)
            {
                await Connection.ConnectAsync();
            }
            else
            {
                log.Debug("NOT LINUX!");
            }
        }

        public Connection Connection { get; }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}