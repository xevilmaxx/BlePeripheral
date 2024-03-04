using LibBlePeripheral.CrossPlatform;
using LibBlePeripheral.LinuxBluezServer.Advertisements;
using LibBlePeripheral.LinuxBluezServer.Core;
using LibBlePeripheral.LinuxBluezServer.Device;
using LibBlePeripheral.LinuxBluezServer.Gatt.Description;
using LibBlePeripheral.LinuxBluezServer.Gatt;
using LibBlePeripheral.LinuxBluezServer.Helpers;
using LibBlePeripheral.WindowsBleServer;
using System;
using System.Threading.Tasks;
using Tmds.DBus;
using System.Collections.Generic;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using LibBlePeripheral.CrossPlatform.MaxStruct;

//https://github.com/exendahal/dotnet_bluez_server
//https://github.com/phylomeno/dotnet-ble-server
namespace LibBlePeripheral.LinuxBluezServer
{
    public class TestLinux : IBlePeripheral
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private MaxBleDevice BleDeviceMetadata { get; set; }

        #region DefaultInit

        //public TestLinux()
        //{
        //    //LegacyIniTWayAsync();
        //    Init();

        //}

        //private async Task LegacyIniTWayAsync()
        //{
        //    using (var serverContext = new ServerContext())
        //    {
        //        var advertisementProperties = new AdvertisementProperties
        //        {
        //            Type = "peripheral",
        //            ServiceUUIDs = new[] { "12345678-1234-5678-1234-56789abcdef0" },
        //            LocalName = "A",
        //        };

        //        await new AdvertisingManager(serverContext).CreateAdvertisement(advertisementProperties, (val) => { });
        //    }
        //}

        public async void InitAsync()
        {

            ServerContext _CurrentServerContext = new ServerContext();
            await _CurrentServerContext.Connect();

            await BleAdvertisement.RegisterAdvertisement(_CurrentServerContext);

            await BleGattApplication.RegisterGattApplication(_CurrentServerContext);

            DeviceManager.SetDevicePropertyListenerAsync(_CurrentServerContext, OnDeviceConnectedAsync);

        }

        private async void OnDeviceConnectedAsync(IDevice1 device, PropertyChanges changes)
        {
            foreach (var change in changes.Changed)
            {
                log.Trace($"{change.Key}:{change.Value}");
            }
        }

        #endregion

        #region InitByStruct

        public async Task RegisterAdvertisement(ServerContext serverContext)
        {
            var advertisementProperties = new AdvertisementProperties
            {
                Type = "peripheral",
                //ServiceUUIDs = BleDeviceMetadata.Services.Select(x => x.UUID).ToArray(),
                //linux seems not beign able to support multiple service advertisements at least when they have 128bit UUIDs
                //so in order to bypass the issue we will keep first service UUID and all characteristics even of other services
                //will go under a unique service
                ServiceUUIDs = new string[] { BleDeviceMetadata.Services.First().UUID },
                LocalName = BleDeviceMetadata.Name
            };
            await new AdvertisingManager(serverContext).CreateAdvertisement(advertisementProperties, OnAdvertisementReceived);
        }

        private void OnAdvertisementReceived(AdvertisementReceivedEventArgs args)
        {
            log.Debug($"Advertisement Data: {args.AdvertisementData}");
            log.Debug($"Device Address: {args.DeviceAddress}");
        }

        private (CharacteristicFlags EnumFlags, string[] StringFlags) DecideCharacteristicProps(MaxGattCharacteristic bleGattChar)
        {

            CharacteristicFlags charactFlags = CharacteristicFlags.None;
            List<string> strFlags = new List<string>() { };
            foreach (var flag in bleGattChar.Flags)
            {
                if (flag == MaxCharactPropType.Read)
                {
                    charactFlags |= CharacteristicFlags.Read;
                    if(strFlags.Contains("read") == false)
                    {
                        strFlags.Add("read");
                    }
                }
                else if (flag == MaxCharactPropType.Write)
                {
                    charactFlags |= CharacteristicFlags.Write;
                    if (strFlags.Contains("write") == false)
                    {
                        strFlags.Add("write");
                    }
                }
                //else if (flag == MaxCharactPropType.WriteWithoutResponse)
                //{
                //    charactFlags |= CharacteristicFlags.Write;
                //    if (strFlags.Contains("write") == false)
                //    {
                //        strFlags.Add("write");
                //    }
                //}
            }

            return (charactFlags, strFlags.ToArray());

        }

        private List<GattDescriptorDescription> CreateCharacteristicDescription(MaxGattCharacteristic bleGattChar, string[] strFlags)
        {

            var describers = new List<GattDescriptorDescription>();

            //description is optional, besides informative content its dont even needed
            if (bleGattChar.Descriptor != null)
            {
                describers.Add(new GattDescriptorDescription
                {
                    Value = bleGattChar.Descriptor.StaticValue,
                    UUID = bleGattChar.Descriptor.UUID,
                    Flags = strFlags.ToArray()
                });
            }

            return describers;

        }

        private async Task<bool> CreateCharacteristic(GattServiceDescription gattServiceDescription, GattApplicationBuilder gab, MaxGattCharacteristic bleGattChar)
        {

            (var charactFlags, var strFlags) = DecideCharacteristicProps(bleGattChar);

            var gattCharacteristicDescription = new GattCharacteristicDescription
            {
                CharacteristicSource = new CharacteristicSourceV2(ref bleGattChar),
                UUID = bleGattChar.UUID,
                Flags = charactFlags
            };

            var describers = CreateCharacteristicDescription(bleGattChar, strFlags);

            gab.AddService(gattServiceDescription).WithCharacteristic(gattCharacteristicDescription, describers.ToArray());

            return true;

        }

        public async Task<bool> CreateService(ServerContext serverContext, MaxGattService Data)
        {

            var gattServiceDescription = new GattServiceDescription
            {
                UUID = Data.UUID,
                Primary = true
            };

            var gab = new GattApplicationBuilder();

            foreach (var bleGattChar in Data.Characteristics)
            {

                await CreateCharacteristic(gattServiceDescription, gab, bleGattChar);

            }

            await new GattApplicationManager(serverContext).RegisterGattApplication(gab.BuildServiceDescriptions());

            return true;

        }

        public async void InitByStructAsync()
        {

            ServerContext _CurrentServerContext = new ServerContext();
            await _CurrentServerContext.Connect();

            await RegisterAdvertisement(_CurrentServerContext);

            foreach (var bleGattSvc in BleDeviceMetadata.Services)
            {

                await CreateService(_CurrentServerContext, bleGattSvc);

            }

            DeviceManager.SetDevicePropertyListenerAsync(_CurrentServerContext, OnDeviceConnectedAsync);

        }

        #endregion

        public IBlePeripheral SetMaxBleDevice(MaxBleDevice Data)
        {
            if (Data.IsStructOk() == true)
            {
                log.Debug("Max Struct is OK");
                BleDeviceMetadata = Data;
            }
            else
            {
                log.Error("Max Struct is KO");
            }

            return this;
        }

        public IBlePeripheral Expose()
        {
            if (BleDeviceMetadata == null)
            {
                InitAsync();
            }
            else
            {
                InitByStructAsync();
            }
            return this;
        }

    }
}
