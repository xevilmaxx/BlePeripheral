using BleMauiTest.DTO;
using Microsoft.AspNetCore.Components;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System.Text;

namespace BleMauiTest.Components.Pages
{
    public partial class BleActOnVicinity : ComponentBase
    {

        private List<DiscoveredDeviceDTO> DiscoveredDevices { get; set; } = new();

        //private IBluetoothLE ble;
        private IAdapter adapter;

        private int counter = 0;
        private string Result { get; set; }

        private Guid? LastCalledGuid = null;
        private bool IsCanCall = true;
        private System.Timers.Timer TimeoutBeforeNextCall = null;

        private List<AdvertisementRecord> AdvertisementTags { get; set; } = new();

        //0 you are 100% close, further you go, biger -X becomes
        private int MinimumAllowedRssi = -70;

        private bool IsCanSendOpenBar = true;
        private bool IsCanSendCode = false;

        private string CustomCode = "CustomCode";

        private int TimeoutBeforeNextScan = 5000;

        private string OpenBarReadResult = "";

        public BleActOnVicinity()
        {

            TimeoutBeforeNextCall = new System.Timers.Timer()
            {
                Interval = 15000,
                AutoReset = false
            };

            TimeoutBeforeNextCall.Elapsed += (_, _) => { IsCanCall = true; };

            adapter = CrossBluetoothLE.Current.Adapter;

            //override defualt
            //yet too few time may dont catch long away devices
            //adapter.ScanTimeout = 2500;

            adapter.DeviceDiscovered += (a, Data) =>
            {
                if (Data.Device != null)
                {
                    lock (DiscoveredDevices)
                    {

                        CheckIfCanUpsertInMemory(Data.Device);

                        GetNearestAdvertisementTag();

                        IfNearEnoughConnectAndSpeak();

                        //if (Data.Device.Name == "Seos")
                        //{

                        //}

                        this.InvokeAsync(() => StateHasChanged());

                    }
                }
            };

            //relaunch scan asap
            //wil be done on stop start
            //adapter.ScanTimeoutElapsed += OnScanTimeoutElapsedAsync;

            adapter.DeviceAdvertised += (a, Data) =>
            {
                lock (DiscoveredDevices)
                {

                    CheckIfCanUpsertInMemory(Data.Device);

                    IfNearEnoughConnectAndSpeak();

                    this.InvokeAsync(() => StateHasChanged());

                    //if (Data.Device.Name == "Seos")
                    //{

                    //}
                }
            };

            adapter.DeviceDisconnected += (a, Data) =>
            {
                lock (DiscoveredDevices)
                {

                    var toRemove = DiscoveredDevices.Where(x => x.Device.Id == Data.Device.Id).FirstOrDefault();
                    if (toRemove != null)
                    {
                        DiscoveredDevices.Remove(toRemove);
                    }

                    this.InvokeAsync(() => StateHasChanged());

                }
            };

            adapter.DeviceConnectionLost += (a, Data) =>
            {
                lock (DiscoveredDevices)
                {

                    var toRemove = DiscoveredDevices.Where(x => x.Device.Id == Data.Device.Id).FirstOrDefault();
                    if (toRemove != null)
                    {
                        DiscoveredDevices.Remove(toRemove);
                    }

                    this.InvokeAsync(() => StateHasChanged());

                }
            };

            //BeginDiscovery();

        }

        private async void OnScanTimeoutElapsedAsync(object sender, EventArgs Data)
        {
            _ = Task.Run(async () =>
            {

                //avoid scanning too frequently as may consume the battery
                Thread.Sleep(TimeoutBeforeNextScan);

                lock (DiscoveredDevices)
                {
                    //eventually clean those who are not more visible
                    //DiscoveredDevices = new List<(IDevice Device, DateTime LastChange)>();
                    var now = DateTime.Now;
                    //instead of emptying remove those not more reachable, for some time
                    for (int i = 0; i < DiscoveredDevices.Count; i++)
                    {
                        if (now.Subtract(DiscoveredDevices[i].LastChange).TotalSeconds >= 15)
                        {
                            DiscoveredDevices.Remove(DiscoveredDevices[i]);
                        }
                    }
                }

                //this.InvokeAsync(() => StateHasChanged());

                //await adapter.StopScanningForDevicesAsync();
                await adapter.StartScanningForDevicesAsync();

            });
        }

        private void CheckIfCanUpsertInMemory(IDevice Device)
        {

            if(Device == null)
            {
                return;
            }

            var existentDevice = DiscoveredDevices.Where(x => x.Device != null && x.Device.Id == Device.Id).FirstOrDefault();

            if (existentDevice == null)
            {
                DiscoveredDevices.Add(new DiscoveredDeviceDTO(Device));
            }
            else
            {
                DiscoveredDevices.Remove(existentDevice);
                DiscoveredDevices.Add(new DiscoveredDeviceDTO(Device));
                //update
                //existentDevice = Device;
            }

        }

        private void GetNearestAdvertisementTag()
        {
            var nearest = DiscoveredDevices.OrderByDescending(x => x.Device.Rssi).FirstOrDefault();
            var nearestAdvTags = nearest.Device?.AdvertisementRecords.ToList();

            //optinal, just for debug
            if (nearestAdvTags != null)
            {
                AdvertisementTags = nearestAdvTags;
            }
        }

        private List<IDevice> GetOnlyPertinentNearestBleDevice()
        {
            lock (DiscoveredDevices)
            {
                return DiscoveredDevices
                    .Where(x =>
                        string.Equals(x.Device.Name, "ZENMAX", StringComparison.CurrentCultureIgnoreCase)
                        ||
                        string.Equals(x.Device.Name, "EvilMax", StringComparison.CurrentCultureIgnoreCase)
                        //||
                        //string.Equals(x.Device.Name, "Seos", StringComparison.CurrentCultureIgnoreCase)
                        )
                    .OrderByDescending(x => x.Device.Rssi)
                    .Select(x => x.Device)
                    .ToList();
            }
        }

        private async void IfNearEnoughConnectAndSpeak()
        {

            if(IsCanCall == false)
            {
                return;
            }

            var nearest = GetOnlyPertinentNearestBleDevice().FirstOrDefault();

            if (nearest != null && nearest.Rssi >= MinimumAllowedRssi)
            {

                try
                {

                    //cant call same within timeout
                    if (IsCanCall == false && LastCalledGuid == nearest.Id)
                    {
                        return;
                    }

                    IsCanCall = false;
                    LastCalledGuid = nearest.Id;
                    TimeoutBeforeNextCall?.Start();

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
                    var services = await nearest.GetServicesAsync();

                    #region AttemptToCommunicateWithHidRfid_NotWorking
                    //var serviceSeos = services.Where(x => x.Id == Guid.Parse("00009800-0000-1000-8000-00177A000002")).FirstOrDefault();
                    //var characteristicSeos = (await serviceSeos?.GetCharacteristicsAsync())?
                    //        //.Where(x => x.Id == Guid.Parse("0000AA00-0000-1000-8000-00177A000002"))
                    //        .Where(x => x.Id == Guid.Parse("00002902-0000-1000-8000-00177A000002"))
                    //        .FirstOrDefault();

                    //var seosResult = await characteristicSeos?.ReadAsync();
                    //var asciiSeosResult = Encoding.ASCII.GetString(seosResult.data);
                    #endregion

                    //the service i need
                    var service = services.Where(x => x.Id == Guid.Parse("00000000-0000-0000-0000-000000000001")).FirstOrDefault();

                    if (IsCanSendOpenBar == true)
                    {
                        var characteristic = (await service?.GetCharacteristicsAsync())?
                            .Where(x => x.Id == Guid.Parse("00000000-0000-0000-0000-000000000002"))
                            .FirstOrDefault();

                        var readResult = await characteristic?.ReadAsync();

                        OpenBarReadResult = Encoding.ASCII.GetString(readResult.data);
                    }

                    if (IsCanSendCode == true)
                    {
                        var characteristic = (await service?.GetCharacteristicsAsync())?
                            .Where(x => x.Id == Guid.Parse("00000000-0000-0000-0000-000000000003"))
                            .FirstOrDefault();

                        await characteristic?.WriteAsync(Encoding.ASCII.GetBytes(CustomCode));
                    }

                    #region GenericCharacteristicTry
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
                    #endregion

                    await adapter.DisconnectDeviceAsync(nearest);

                    Result = counter + " - OK";

                }
                catch (Exception ex)
                {
                    Result = counter + " - KO";
                }
            }
        }

        private List<IDevice> GuiDisplayNearBleDevices()
        {
            bool isCandisplayAll = true;
            if (isCandisplayAll == false)
            {
                return GetOnlyPertinentNearestBleDevice();
            }
            else
            {
                return DiscoveredDevices.OrderByDescending(x => x.Device.Rssi)
                    .Select(x => x.Device)
                    .ToList();
            }
        }

        //public void NewDeviceDiscovered(object sender, DeviceEventArgs Data)
        //{

        //}

        public async void BeginDiscovery()
        {

            //ble = CrossBluetoothLE.Current;

            adapter.ScanTimeoutElapsed += OnScanTimeoutElapsedAsync;
            await adapter.StartScanningForDevicesAsync();

            //Task.Run(async () =>
            //{
            //    Thread.Sleep(5000);
            //    await adapter.StopScanningForDevicesAsync();
            //});

        }

        public async void StopDiscovery()
        {

            //ble = CrossBluetoothLE.Current;

            try
            {

                adapter.ScanTimeoutElapsed -= OnScanTimeoutElapsedAsync;

            }
            catch (Exception ex)
            {

            }

            await adapter.StopScanningForDevicesAsync();

            //Task.Run(async () =>
            //{
            //    Thread.Sleep(5000);
            //    await adapter.StopScanningForDevicesAsync();
            //});

        }

        public async void SendHello()
        {

            counter++;

            IfNearEnoughConnectAndSpeak();

            StateHasChanged();

        }

    }
}
