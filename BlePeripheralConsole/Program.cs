using LibBlePeripheral;
using LibBlePeripheral.CrossPlatform;
using System;
using System.Configuration;
using System.Text;
using System.Threading;

namespace BlePeripheralConsole
{
    internal class Program
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private static ManualResetEvent resetEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {

            Console.WriteLine("Hello, World!");

            var maxStruct = new MaxStructBuilder()
                //.BuildTest1Service();
                //.BuildTest2Services();
                .BuildTestV3_ExternalActions(
                    (data) =>
                    {
                        Console.WriteLine("ììììì ----->" + Encoding.ASCII.GetString(data));
                    },
                    () =>
                    {
                        //var result = CoreLiteGate.OpenForwardBar(new IPC.SHARED.NoArgs() { });
                        return Encoding.ASCII.GetBytes($"okkkkkk");
                    }
                );

            var ble = new BlePeripheralFactory()
                .GetDriver()
                .SetMaxBleDevice(maxStruct)
                .Expose();

            Console.CancelKeyPress += (sender, eventArgs) => resetEvent.Set();
            resetEvent.WaitOne();

            log.Debug("BlePeripheral ended! This normally shouldnt happen ...");

        }
    }
}
