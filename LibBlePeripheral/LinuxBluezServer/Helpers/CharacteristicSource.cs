using LibBlePeripheral.LinuxBluezServer.Gatt;
using System;
using System.Text;
using System.Threading.Tasks;

namespace LibBlePeripheral.LinuxBluezServer.Helpers
{
    public class CharacteristicSource : ICharacteristicSource
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public override Task ConfirmAsync()
        {
            return Task.CompletedTask;
        }

        public override Task<byte[]> ReadValueAsync(string objectPath)
        {

            TaskCompletionSource<byte[]> tcs = new();

            try
            {

                tcs.TrySetResult(Encoding.ASCII.GetBytes($"Read Operation"));

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return tcs.Task;

        }

        public override Task StartNotifyAsync()
        {
            return Task.CompletedTask;
        }

        public override Task StopNotifyAsync()
        {
            return Task.CompletedTask;
        }

        public override Task WriteValueAsync(byte[] value, bool response, string objectPath)
        {
            try
            {

                string dataString = Encoding.UTF8.GetString(value);

                log.Debug($"Write from central: {dataString}");

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return Task.CompletedTask;
        }
    }
}