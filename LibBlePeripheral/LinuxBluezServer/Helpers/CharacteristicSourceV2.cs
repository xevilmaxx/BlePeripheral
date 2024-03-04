using LibBlePeripheral.CrossPlatform.MaxStruct;
using LibBlePeripheral.LinuxBluezServer.Gatt;
using System.Text;

namespace LibBlePeripheral.LinuxBluezServer.Helpers
{
    public class CharacteristicSourceV2 : ICharacteristicSource
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private MaxGattCharacteristic CharacteristicStructPiece { get; set; }

        public CharacteristicSourceV2(ref MaxGattCharacteristic Data)
        {
            CharacteristicStructPiece = Data;
        }

        private string ByteArrayPrettyPrint(byte[] resp)
        {
            string temp = "";
            resp.ToList().ForEach(j => temp += " 0x" + j.ToString("X2"));
            return temp;
        }

        public override Task ConfirmAsync()
        {
            return Task.CompletedTask;
        }

        public override Task<byte[]> ReadValueAsync(string objectPath)
        {

            TaskCompletionSource<byte[]> tcs = new();

            try
            {

                if(CharacteristicStructPiece?.Descriptor?.StaticValue != null)
                {
                    tcs.TrySetResult(CharacteristicStructPiece.Descriptor.StaticValue);
                    log.Debug($"Answered ASCII: {Encoding.ASCII.GetString(CharacteristicStructPiece.Descriptor.StaticValue)}");
                    log.Debug($"Answered Bytes -> {ByteArrayPrettyPrint(CharacteristicStructPiece.Descriptor.StaticValue)}");
                }
                else if (CharacteristicStructPiece?.OnReadCaptured != null)
                {
                    var bytes = CharacteristicStructPiece.OnReadCaptured.Invoke();
                    tcs.TrySetResult(bytes);
                    log.Debug($"Answered ASCII: {Encoding.ASCII.GetString(bytes)}");
                    log.Debug($"Answered Bytes -> {ByteArrayPrettyPrint(bytes)}");
                }
                else
                {
                    var bytes = Encoding.ASCII.GetBytes($"Read Operation");
                    tcs.TrySetResult(bytes);
                    log.Debug($"Answered ASCII: {Encoding.ASCII.GetString(bytes)}");
                    log.Debug($"Answered Bytes -> {ByteArrayPrettyPrint(bytes)}");
                }

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

                log.Debug($"Received ASCII: {Encoding.ASCII.GetString(value)}");
                log.Debug($"Received Bytes -> {ByteArrayPrettyPrint(value)}");

                if (CharacteristicStructPiece?.OnWriteCaptured != null)
                {

                    CharacteristicStructPiece.OnWriteCaptured.Invoke(value);

                }
                else
                {

                    //string dataString = Encoding.UTF8.GetString(value);

                    //log.Debug($"Write from central: {dataString}");

                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return Task.CompletedTask;
        }
    }
}