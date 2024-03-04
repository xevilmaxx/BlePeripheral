using InTheHand.Bluetooth;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace BleMauiTest.Components.Pages
{
    public partial class Counter : ComponentBase
    {

        private List<BluetoothAdvertisingEvent> DiscoveredDevices { get; set; } = new();

        private int currentCount = 0;

        public Counter()
        {
            Bluetooth.AdvertisementReceived += AdvertisementReceived;
            _ = GetDevices();
        }

        private async void IncrementCount()
        {
            currentCount++;

            //get nearest
            var nearest = DiscoveredDevices.OrderByDescending(x => x.Rssi).FirstOrDefault();
            if (nearest != null && nearest.Device.Name == "Seos")
            {

                try
                {

                    //nearest.Device.Gatt.AutoConnect = false;

                    //works but keeps trying to connect indefinitvely unitl Disconnect is done
                    //once Disconnect is done we cannot connect again though!
                    await nearest.Device.Gatt.ConnectAsync();

                    //disposes every shit, not ok
                    //nearest.Device.Gatt.Disconnect();

                }
                catch (Exception ex)
                {

                }

            }

        }

        private async void IncrementCountAdvanced()
        {
            currentCount++;

            //get nearest
            var nearest = DiscoveredDevices.OrderByDescending(x => x.Rssi).FirstOrDefault();
            if (nearest != null && nearest.Device.Name == "Seos")
            {

                try
                {

                    //nearest.Device.Gatt.AutoConnect = true;

                    await nearest.Device.Gatt.ConnectAsync();


                    var svc = await nearest.Device.Gatt.GetPrimaryServicesAsync();

                    //foreach (var service in svc)
                    //{
                    //    var characteristics = await service.GetCharacteristicsAsync();
                    //    if (characteristics != null && characteristics.Count > 0)
                    //    {
                    //        var codeBytes = UTF8Encoding.UTF8.GetBytes("abcdefghlmnopqrstuv");

                    //        await characteristics.First().WriteValueWithoutResponseAsync(codeBytes);
                    //    }
                    //}

                    var goodService = svc.Where(x => x.Uuid.ToString() == "00009800-0000-1000-8000-00177a000002").First();

                    var characteristic = await goodService.GetCharacteristicsAsync();

                    var codeBytes = Encoding.ASCII.GetBytes("abcdefghlmnopqrstuv");

                    await characteristic.First().WriteValueWithoutResponseAsync(codeBytes);

                    //Thread.Sleep(5000);

                    //nearest.Device.Gatt.Disconnect();

                }
                catch (Exception ex)
                {

                }

            }

        }

        private void AdvertisementReceived(object sender, BluetoothAdvertisingEvent Data)
        {

            /* Unmerged change from project 'BleMauiTest (net8.0-ios)'
            Before:
                        Console.WriteLine($"Found  ---: {Data.Rssi}");


                        if (Data.Device != null)
            After:
                        Console.WriteLine($"Found  ---: {Data.Rssi}");


                        if (Data.Device != null)
            */

            /* Unmerged change from project 'BleMauiTest (net8.0-android)'
            Before:
                        Console.WriteLine($"Found  ---: {Data.Rssi}");


                        if (Data.Device != null)
            After:
                        Console.WriteLine($"Found  ---: {Data.Rssi}");


                        if (Data.Device != null)
            */

            /* Unmerged change from project 'BleMauiTest (net8.0-windows10.0.19041.0)'
            Before:
                        Console.WriteLine($"Found  ---: {Data.Rssi}");


                        if (Data.Device != null)
            After:
                        Console.WriteLine($"Found  ---: {Data.Rssi}");


                        if (Data.Device != null)
            */
            Console.WriteLine($"Found  ---: {Data.Rssi}");


            if (Data.Device != null)
            {

                var existentDevice = DiscoveredDevices.Where(x => x != null && x.Device.Id == Data.Device.Id).FirstOrDefault();

                if (existentDevice == null)
                {
                    DiscoveredDevices.Add(Data);
                }
                else if (
                        existentDevice != null
                        &&
                        existentDevice.Device.Gatt.IsConnected == false
                    )
                {
                    existentDevice = Data;
                }

            }
        }

        private async Task GetDevices()
        {

            _ = Task.Run(async () =>
            {
                var devices = await Bluetooth.RequestLEScanAsync(new BluetoothLEScanOptions()
                {
                    AcceptAllAdvertisements = true,
                    KeepRepeatedDevices = false
                });

                //Thread.Sleep(5000);

                //devices.Stop();
            });

            //Console.WriteLine("");
        }

    }
}
