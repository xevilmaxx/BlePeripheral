using LibBlePeripheral.LinuxBluezServer.Advertisements;
using LibBlePeripheral.LinuxBluezServer.Core;
using System;
using System.Threading.Tasks;

namespace LibBlePeripheral.LinuxBluezServer.Helpers
{
    public class BleAdvertisement
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public static async Task RegisterAdvertisement(ServerContext serverContext)
        {
            var advertisementProperties = new AdvertisementProperties
            {
                Type = "peripheral",
                ServiceUUIDs = new[] { BleParam.SERVICE_UUID },
                LocalName = BleParam.DEVICE_NAME
            };
            await new AdvertisingManager(serverContext).CreateAdvertisement(advertisementProperties, OnAdvertisementReceived);
        }

        private static void OnAdvertisementReceived(AdvertisementReceivedEventArgs args)
        {
            log.Debug(args.AdvertisementData);
            log.Debug(args.DeviceAddress);
        }

    }
}