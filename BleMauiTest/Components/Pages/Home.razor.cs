using Microsoft.AspNetCore.Components;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace BleMauiTest.Components.Pages
{
    public partial class Home : ComponentBase
    {

        private List<IDevice> DiscoveredDevices { get; set; } = new();

        //private IBluetoothLE ble;
        private IAdapter adapter;

        private int counter = 0;
        private string Result { get; set; }

        private List<AdvertisementRecord> AdvertisementTags { get; set; } = new();

        public Home()
        {

            //BeginDiscovery();

        }

        public void NewDeviceDiscovered(object sender, DeviceEventArgs Data)
        {
            if (Data.Device != null)
            {
                lock (DiscoveredDevices)
                {
                    var existentDevice = DiscoveredDevices.Where(x => x != null && x.Id == Data.Device.Id).FirstOrDefault();

                    if (existentDevice == null)
                    {

                        DiscoveredDevices.Add(Data.Device);

                        var nearest = DiscoveredDevices.OrderByDescending(x => x.Rssi).FirstOrDefault();
                        var nearestAdvTags = nearest?.AdvertisementRecords.ToList();

                        //optinal, just for debug
                        if (nearestAdvTags != null)
                        {
                            AdvertisementTags = nearestAdvTags;
                        }

                        this.InvokeAsync(() => StateHasChanged());

                    }
                }
            }
        }

        public async void BeginDiscovery()
        {

            //ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;

            adapter.DeviceDiscovered += NewDeviceDiscovered;
            adapter.StartScanningForDevicesAsync();

            Task.Run(async () =>
            {
                Thread.Sleep(5000);
                await adapter.StopScanningForDevicesAsync();
            });

        }

        public async void SendHello()
        {

            counter++;

            var nearest = DiscoveredDevices.OrderByDescending(x => x.Rssi).FirstOrDefault();
            if (nearest != null && (nearest.Name == "Seos"))
            {

                try
                {

                    //optinal, just for debug
                    //if (AdvertisementTags.Count <= 0)
                    //{
                    //    AdvertisementTags = nearest.AdvertisementRecords.ToList();
                    //}

                    //on connect BLE will receive cur device MAC, and i think its impossible to change!
                    //Android especially has this fucking privacy feature which rotates Bluetooth MAC which is pain in the ass
                    //at least NFC seems stable
                    await adapter.ConnectToDeviceAsync(nearest);

                    //changes really nothing
                    //var services = await nearest.GetServicesAsync();

                    //foreach (var svc in services.Skip(1))
                    //{
                    //    var characteristics = await svc.GetCharacteristicsAsync();
                    //    foreach (var chr in characteristics)
                    //    {
                    //        if (chr.CanWrite == true)
                    //        {
                    //            await chr.WriteAsync(Encoding.ASCII.GetBytes("HelloWorld"));
                    //        }
                    //    }
                    //}

                    await adapter.DisconnectDeviceAsync(nearest);

                    Result = counter + " - OK";

                }
                catch (Exception ex)
                {
                    Result = counter + " - KO";
                }

            }

            StateHasChanged();

        }

    }
}
