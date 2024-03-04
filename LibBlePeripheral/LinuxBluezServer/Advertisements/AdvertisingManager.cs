using LibBlePeripheral.LinuxBluezServer.Constants;
using LibBlePeripheral.LinuxBluezServer.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace LibBlePeripheral.LinuxBluezServer.Advertisements
{
    public class AdvertisementReceivedEventArgs : EventArgs
    {
        public string DeviceAddress { get; }
        public IDictionary<string, object> AdvertisementData { get; }

        public AdvertisementReceivedEventArgs(string deviceAddress, IDictionary<string, object> advertisementData)
        {
            DeviceAddress = deviceAddress;
            AdvertisementData = advertisementData;
        }
    }
    public class AdvertisingManager
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private readonly ServerContext _Context;
        public event EventHandler<AdvertisementReceivedEventArgs> AdvertisementReceived;
        protected virtual void OnAdvertisementReceived(AdvertisementReceivedEventArgs e)
        {
            AdvertisementReceived?.Invoke(this, e);
        }

        public AdvertisingManager(ServerContext context)
        {
            _Context = context;
        }

        public async Task RegisterAdvertisement(Advertisement advertisement, Action<AdvertisementReceivedEventArgs> onAdvertisementReceived)
        {
            var advertisementExists = await GetAdvertisingManager().GetAsync<byte>("ActiveInstances");
            if (advertisementExists == 0)
            {
                // Subscribe to the AdvertisementReceived event with the provided action
                AdvertisementReceived += (sender, args) => onAdvertisementReceived?.Invoke(args);
                await _Context.Connection.RegisterObjectAsync(advertisement);
                log.Debug($"Advertisement object {advertisement.ObjectPath} created");

                await GetAdvertisingManager().RegisterAdvertisementAsync(
                    ((IDBusObject)advertisement).ObjectPath,
                    new Dictionary<string, object>()
                );
                log.Debug($"Advertisement {advertisement.ObjectPath} registered in BlueZ advertising manager");
            }
        }
        private ILEAdvertisingManager1 GetAdvertisingManager()
        {
            return _Context.Connection.CreateProxy<ILEAdvertisingManager1>(
                BlueZConstants.BASE_PATH,
                BlueZConstants.ADAPTER_PATH
            );
        }

        public async Task CreateAdvertisement(AdvertisementProperties advertisementProperties, Action<AdvertisementReceivedEventArgs> onAdvertisementReceived)
        {
            var advertisement = new Advertisement(
                BlueZConstants.ADVERTISEMENT_PATH,
                advertisementProperties
            );
            await new AdvertisingManager(_Context).RegisterAdvertisement(advertisement, onAdvertisementReceived);
        }
    }
}
