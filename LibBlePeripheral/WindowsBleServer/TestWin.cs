using LibBlePeripheral.CrossPlatform;
using LibBlePeripheral.CrossPlatform.MaxStruct;
using System.Text;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace LibBlePeripheral.WindowsBleServer
{
    public class TestWin : IBlePeripheral
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private MaxBleDevice BleDeviceMetadata { get; set; }

        #region DefaultInit

        private async Task InitAsync()
        {
            try
            {

                ///var bluetoothAdapter = await BluetoothAdapter.GetDefaultAsync();

                //var watcher = bluetoothAdapter.BluetoothAddress;

                GattServiceProviderResult result = await GattServiceProvider.CreateAsync(Guid.Parse("00000000-0000-0000-0000-000000000001"));

                GattServiceProvider serviceProvider = null;
                if (result.Error == BluetoothError.Success)
                {
                    serviceProvider = result.ServiceProvider;
                    // 
                }

                AddReadCharacteristicsToGattServerAsync(serviceProvider);

                AddWriteCharacteristicsToGattServerAsync(serviceProvider);

                Expose(serviceProvider);

                DisconnectClients();

                AddDeviceWatcher();

            }
            catch (Exception ex)
            {

            }
        }

        private void AddDeviceWatcher()
        {

            // Query for extra properties you want returned
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            DeviceWatcher deviceWatcher = DeviceInformation.CreateWatcher(
                BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                requestedProperties,
                DeviceInformationKind.AssociationEndpoint
                );

            //var myId = "";

            // Register event handlers before starting the watcher.
            // Added, Updated and Removed are required to get all nearby devices
            deviceWatcher.Added += (DeviceWatcher, Device) =>
            {
                log.Trace($"Added DeviceId: {Device?.Id}, DeviceName: {Device?.Name}");
                //if (Device.Name.Contains("HUAWEI Y6") == true)
                //{
                //    myId = Device.Id;
                //}
            };
            deviceWatcher.Updated += (DeviceWatcher, Device) =>
            {
                log.Trace($"Updated: {Device?.Id}");
                //if (Device.Id == myId)
                //{

                //}
            };
            deviceWatcher.Removed += (DeviceWatcher, Device) =>
            {
                log.Trace($"Removed: {Device?.Id}");
                //if (Device.Id == myId)
                //{

                //}
            };

            // EnumerationCompleted and Stopped are optional to implement.
            //deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            //deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start the watcher.
            deviceWatcher.Start();

        }

        private void DisconnectClients()
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        string filter = BluetoothLEDevice.GetDeviceSelectorFromConnectionStatus(BluetoothConnectionStatus.Connected);
                        DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(filter);

                        foreach (DeviceInformation d in devices)
                        {
                            BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(d.Id);
                            var gattServices = await device.GetGattServicesAsync();
                            foreach (var service in gattServices.Services)
                            {
                                if (service.Session.SessionStatus == GattSessionStatus.Active)
                                {
                                    service.Session.Dispose();
                                }

                                service.Dispose();
                            }
                            device.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        Thread.Sleep(15000);
                    }
                }
            });
        }

        private async void Expose(GattServiceProvider serviceProvider)
        {
            //publish
            GattServiceProviderAdvertisingParameters advParameters = new GattServiceProviderAdvertisingParameters
            {
                IsDiscoverable = true,
                IsConnectable = true
            };
            serviceProvider.StartAdvertising(advParameters);
        }

        private async void AddReadCharacteristicsToGattServerAsync(GattServiceProvider serviceProvider)
        {
            //read characterisitc
            var readParameters = new GattLocalCharacteristicParameters
            {
                CharacteristicProperties = GattCharacteristicProperties.Read,
                //StaticValue = (new byte[] { 0x05 }).AsBuffer(),
                ReadProtectionLevel = GattProtectionLevel.Plain,
            };

            var characteristicResult = await serviceProvider.Service.CreateCharacteristicAsync(Guid.Parse("00000000-0000-0000-0000-000000000002"), readParameters);
            if (characteristicResult.Error != BluetoothError.Success)
            {
                // An error occurred.
                return;
            }
            var _readCharacteristic = characteristicResult.Characteristic;
            _readCharacteristic.ReadRequested += ReadCharacteristic_ReadRequested;
        }

        private async void AddWriteCharacteristicsToGattServerAsync(GattServiceProvider serviceProvider)
        {
            //write characterisitc
            var writeParameters = new GattLocalCharacteristicParameters
            {
                CharacteristicProperties = GattCharacteristicProperties.WriteWithoutResponse,
                //StaticValue = (new byte[] { 0x05 }).AsBuffer(),
                ReadProtectionLevel = GattProtectionLevel.Plain,
            };

            var characteristicResult = await serviceProvider.Service.CreateCharacteristicAsync(Guid.Parse("00000000-0000-0000-0000-000000000003"), writeParameters);
            if (characteristicResult.Error != BluetoothError.Success)
            {
                // An error occurred.
                return;
            }
            var _writeCharacteristic = characteristicResult.Characteristic;
            _writeCharacteristic.WriteRequested += WriteCharacteristic_WriteRequested;
        }

        #endregion

        #region Read_Write_Handlers

        private string ByteArrayPrettyPrint(byte[] resp)
        {
            string temp = "";
            resp.ToList().ForEach(j => temp += " 0x" + j.ToString("X2"));
            return temp;
        }

        private async void ReadCharacteristic_ReadRequested(GattLocalCharacteristic sender, GattReadRequestedEventArgs args)
        {
            try
            {

                var deferral = args.GetDeferral();

                // Our familiar friend - DataWriter.
                var writer = new DataWriter();

                var staticAnswer = Encoding.ASCII.GetBytes("DefaultReadAnswer");

                // populate writer w/ some data. 
                // ... 
                writer.WriteBytes(staticAnswer);

                var request = await args.GetRequestAsync();
                request.RespondWithValue(writer.DetachBuffer());

                deferral.Complete();

                log.Debug($"Answered ASCII: {Encoding.ASCII.GetString(staticAnswer)}");
                log.Debug($"Answered Bytes -> {ByteArrayPrettyPrint(staticAnswer)}");

            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }

        private async void ReadCharacteristic_ReadRequested(GattLocalCharacteristic sender, GattReadRequestedEventArgs args, byte[] Data)
        {
            try
            {

                var deferral = args.GetDeferral();

                // Our familiar friend - DataWriter.
                var writer = new DataWriter();
                // populate writer w/ some data. 
                // ... 
                writer.WriteBytes(Data);

                var request = await args.GetRequestAsync();
                request.RespondWithValue(writer.DetachBuffer());

                deferral.Complete();

                log.Debug($"Answered ASCII: {Encoding.ASCII.GetString(Data)}");
                log.Debug($"Answered Bytes -> {ByteArrayPrettyPrint(Data)}");

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async void ReadCharacteristic_WithFunc(GattLocalCharacteristic sender, GattReadRequestedEventArgs args, Func<byte[]> CustomFunc)
        {
            try
            {

                var deferral = args.GetDeferral();

                // Our familiar friend - DataWriter.
                var writer = new DataWriter();

                var dynamicAnswer = CustomFunc.Invoke();

                // populate writer w/ some data. 
                // ... 
                writer.WriteBytes(dynamicAnswer);

                var request = await args.GetRequestAsync();
                request.RespondWithValue(writer.DetachBuffer());

                deferral.Complete();

                log.Debug($"Answered ASCII: {Encoding.ASCII.GetString(dynamicAnswer)}");
                log.Debug($"Answered Bytes -> {ByteArrayPrettyPrint(dynamicAnswer)}");

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async void WriteCharacteristic_WriteRequested(GattLocalCharacteristic sender, GattWriteRequestedEventArgs args)
        {
            try
            {

                var deferral = args.GetDeferral();

                var request = await args.GetRequestAsync();
                var reader = DataReader.FromBuffer(request.Value);

                // Parse data as necessary. 
                // Get the remaining buffer length
                int remainingBytes = (int)reader.UnconsumedBufferLength;

                // Allocate an array based on the remaining buffer length
                byte[] data = new byte[remainingBytes];

                // Read all remaining bytes into the array
                reader.ReadBytes(data);

                if (request.Option == GattWriteOption.WriteWithResponse)
                {
                    request.Respond();
                }

                deferral.Complete();

                log.Debug($"Received ASCII: {Encoding.ASCII.GetString(data)}");
                log.Debug($"Received Bytes -> {ByteArrayPrettyPrint(data)}");

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async void WriteCharacteristic_WithAction(GattLocalCharacteristic sender, GattWriteRequestedEventArgs args, Action<byte[]> WriteAction)
        {
            try
            {

                var deferral = args.GetDeferral();

                var request = await args.GetRequestAsync();
                var reader = DataReader.FromBuffer(request.Value);

                // Parse data as necessary. 
                // Get the remaining buffer length
                int remainingBytes = (int)reader.UnconsumedBufferLength;

                // Allocate an array based on the remaining buffer length
                byte[] data = new byte[remainingBytes];

                // Read all remaining bytes into the array
                reader.ReadBytes(data);

                if (request.Option == GattWriteOption.WriteWithResponse)
                {
                    request.Respond();
                }

                deferral.Complete();

                WriteAction.Invoke(data);

                log.Debug($"Received ASCII: {Encoding.ASCII.GetString(data)}");
                log.Debug($"Received Bytes -> {ByteArrayPrettyPrint(data)}");

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        #endregion

        #region InitByStruct

        private GattCharacteristicProperties DecideCharacteristicProps(MaxGattCharacteristic bleGattChar)
        {

            GattCharacteristicProperties gattCharProps = GattCharacteristicProperties.None;

            foreach (var propType in bleGattChar.Flags)
            {
                if (propType == MaxCharactPropType.Read)
                {
                    gattCharProps |= GattCharacteristicProperties.Read;
                }
                else if (propType == MaxCharactPropType.Write)
                {
                    gattCharProps |= GattCharacteristicProperties.Write;
                }
                //else if (propType == MaxCharactPropType.WriteWithoutResponse)
                //{
                //    gattCharProps |= GattCharacteristicProperties.WriteWithoutResponse;
                //}
            }

            return gattCharProps;

        }

        private void CharacteristicAddHandlers(GattLocalCharacteristicResult characteristicResult, MaxGattCharacteristic bleGattChar)
        {

            var _characteristic = characteristicResult.Characteristic;
            if (bleGattChar.Flags.Contains(MaxCharactPropType.Read))
            {
                //static value is prioritized over custom function
                if (
                    bleGattChar.Descriptor != null
                    &&
                    bleGattChar.Descriptor.StaticValue != null
                    &&
                    bleGattChar.Descriptor.StaticValue.Length > 0
                   )
                {
                    _characteristic.ReadRequested += (sender, args) =>
                    {
                        ReadCharacteristic_ReadRequested(sender, args, bleGattChar.Descriptor.StaticValue);
                    };
                }
                else
                {
                    //execute custom function if defined
                    if(bleGattChar.OnReadCaptured != null)
                    {
                        _characteristic.ReadRequested += (sender, args) =>
                        {
                            ReadCharacteristic_WithFunc(sender, args, bleGattChar.OnReadCaptured);
                        };
                    }
                    else
                    {
                        //execute default read handling
                        _characteristic.ReadRequested += ReadCharacteristic_ReadRequested;
                    }
                }
            }
            if (
                bleGattChar.Flags.Contains(MaxCharactPropType.Write) 
                //||
                //bleGattChar.Flags.Contains(MaxCharactPropType.WriteWithoutResponse
                //)
               )
            {
                if(bleGattChar.OnWriteCaptured != null)
                {
                    _characteristic.WriteRequested += (sender, args) => {
                        WriteCharacteristic_WithAction(sender, args, bleGattChar.OnWriteCaptured);
                    };
                }
                else
                {
                    _characteristic.WriteRequested += WriteCharacteristic_WriteRequested;
                }
            }

        }

        private async Task CreateCharacteristic(GattServiceProvider serviceProvider, MaxGattCharacteristic bleGattChar)
        {

            var gattCharProps = DecideCharacteristicProps(bleGattChar);

            //characterisitc
            var characteristicCfg = new GattLocalCharacteristicParameters
            {
                CharacteristicProperties = gattCharProps,
                ReadProtectionLevel = GattProtectionLevel.Plain,
            };

            var characteristicResult = await serviceProvider.Service.CreateCharacteristicAsync(
                Guid.Parse(bleGattChar.UUID),
                characteristicCfg
                );

            if (characteristicResult.Error != BluetoothError.Success)
            {
                // An error occurred.
                return;
            }

            CharacteristicAddHandlers(characteristicResult, bleGattChar);

        }

        private async Task CreateService(MaxGattService bleGattSvc)
        {

            GattServiceProviderResult result = await GattServiceProvider.CreateAsync(Guid.Parse(bleGattSvc.UUID));

            GattServiceProvider serviceProvider = null;
            if (result.Error == BluetoothError.Success)
            {
                serviceProvider = result.ServiceProvider;
            }

            foreach (var bleGattChar in bleGattSvc.Characteristics)
            {

                await CreateCharacteristic(serviceProvider, bleGattChar);

            }

            Expose(serviceProvider);

        }

        private async Task InitByStructAsync()
        {
            try
            {

                foreach (var bleGattSvc in BleDeviceMetadata.Services)
                {

                    await CreateService(bleGattSvc);

                }

                AddDeviceWatcher();

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        #endregion

        public IBlePeripheral SetMaxBleDevice(MaxBleDevice Data)
        {
            if(Data.IsStructOk() == true)
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
