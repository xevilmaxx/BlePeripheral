using LibBlePeripheral.LinuxBluezServer.Core;
using System;
using System.Threading.Tasks;

namespace LibBlePeripheral.LinuxBluezServer.Advertisements
{
    public class Advertisement : PropertiesBase<AdvertisementProperties>, ILEAdvertisement1
    {
        public Advertisement(string objectPath, AdvertisementProperties properties) : base(objectPath, properties)
        {

        }
        public Task ReleaseAsync()
        {
            throw new NotImplementedException();
        }
    }
}