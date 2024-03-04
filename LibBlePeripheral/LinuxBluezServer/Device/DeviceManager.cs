using LibBlePeripheral.LinuxBluezServer.Constants;
using LibBlePeripheral.LinuxBluezServer.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Tmds.DBus;

namespace LibBlePeripheral.LinuxBluezServer.Device
{
    public static class DeviceManager
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public static void SetOnDeviceAddedListener(ServerContext context, Action<(ObjectPath @object, IDictionary<string, IDictionary<string, object>> interfaces)> action)
        {
            var bluezObject = context.Connection.CreateProxy<IObjectManager>(BlueZConstants.BASE_PATH, "/");
            bluezObject.WatchInterfacesAddedAsync(action);
        }

        public static async void SetDevicePropertyListenerAsync(ServerContext context, Action<IDevice1, PropertyChanges> handler)
        {
            var adapter = AdapterManager.CurrentAdapter ?? AdapterManager.GetAdapter(context);
            await adapter.WatchPropertiesAsync(AdapterEvent);
            var devices = await GetDeviceListAsync(context);
            foreach (var device in devices)
            {
                await device.WatchPropertiesAsync(changes => handler(device, changes));
            }

            async void OnDeviceAddedAsync((ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfaces) args)
            {
                log.Debug($"Object path: {args.objectPath}");
                if (BlueZManager.IsMatch(BlueZConstants.IDEVICE_PATH, args.objectPath, args.interfaces, adapter))
                {
                    IDevice1 device1 = context.Connection.CreateProxy<IDevice1>(BlueZConstants.BASE_PATH, args.objectPath);
                    foreach (var item in args.interfaces)
                    {
                        log.Debug($"Key: {item.Key}");
                    }
                    void overriddenHandler(IDevice1 device, PropertyChanges propertyChanges)
                    {
                        foreach (var change in propertyChanges.Changed)
                        {
                            log.Debug($"{change.Key}:{change.Value}");
                        }
                        handler(device, propertyChanges);
                    }
                    await device1.WatchPropertiesAsync(changes => overriddenHandler(device1, changes));

                }
            }
            SetOnDeviceAddedListener(context, OnDeviceAddedAsync);
        }
        private static void AdapterEvent(PropertyChanges changes)
        {
            foreach (var change in changes.Changed)
            {
                log.Debug(change.Key);
            }
        }

        public static async Task<Dictionary<ObjectPath, IDevice1>> GetDeviceDictionaryListAsync(ServerContext context)
        {
            var devices = new Dictionary<ObjectPath, IDevice1>();
            var bluezObject = context.Connection.CreateProxy<IObjectManager>(BlueZConstants.BASE_PATH, "/");
            var ifaceList = await bluezObject.GetManagedObjectsAsync();

            foreach (var entry in ifaceList)
            {
                var path = entry.Key;
                var interfaces = entry.Value;
                if (interfaces.ContainsKey(BlueZConstants.IDEVICE_PATH))
                {
                    var device = context.Connection.CreateProxy<IDevice1>(BlueZConstants.BASE_PATH, path);
                    devices[path] = device;
                }
            }
            return devices;
        }
        public static async Task<List<IDevice1>> GetDeviceListAsync(ServerContext context)
        {
            List<IDevice1> list = new();
            var devices = await GetDeviceDictionaryListAsync(context);
            foreach (var device in devices)
            {
                list.Add(device.Value);
            }
            return list;
        }

        public static async Task<List<IDevice1>> GetDevicePairedListAsync(ServerContext context)
        {
            List<IDevice1> list = new();
            var devices = await GetDeviceDictionaryListAsync(context);
            foreach (var device in devices)
            {
                var paired = await device.Value.GetPairedAsync();
                if (paired)
                {
                    list.Add(device.Value);
                }
            }
            return list;
        }
        public static async Task<bool> PairDeviceAsync(ServerContext context, IDevice1 device)
        {
            var info = await device.GetAllAsync();
            if (info != null)
            {
                try
                {
                    var deviceExist = await CheckDeviceByAddress(context, info.Address);
                    var agentManager = context.Connection.CreateProxy<IAgentManager1>(BlueZConstants.BASE_PATH, new ObjectPath(BlueZConstants.BLUEZ_OBJ));
                    var paths = await FindDevicePathAsync(context, device);
                    foreach (var path in paths)
                    {
                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            // Unregister the agent
                            try
                            {
                                await agentManager.UnregisterAgentAsync(new ObjectPath(path));
                                log.Debug("Agent unregistered successfully.");
                            }
                            catch (Exception ex)
                            {
                                log.Debug($"Error unregistering agent: {ex.Message}");
                            }

                            // Register the agent again
                            try
                            {
                                await agentManager.RegisterAgentAsync(new ObjectPath(path), "NoInputNoOutput");
                                log.Debug("Agent registered successfully.");
                            }
                            catch (Exception ex)
                            {
                                log.Debug($"Error registering agent: {ex.Message}");
                            }
                        }

                        if (!info.Paired)
                        {
                            await device.PairAsync();
                        }
                    }
                }
                catch (DBusException ex)
                {
                    log.Debug($"Pairing failed: {ex.Message}");
                }
            }
            return await device.GetPairedAsync();
        }

        public static async Task<bool> RemoveDeviceAsync(ServerContext context, IDevice1 device)
        {
            if (device != null)
            {
                var paths = await FindDevicePathAsync(context, device);
                foreach (var path in paths)
                {
                    log.Debug($"Removing device: {path}");
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        await AdapterManager.RemoveDeviceAsync(context, new ObjectPath(path));
                        return true;
                    }
                }
            }
            return false;
        }

        public static async Task CancelPairingAsync(IDevice1 device)
        {
            await device.CancelPairingAsync();
        }
        static async Task<List<string>> FindDevicePathAsync(ServerContext context, IDevice1 device)
        {
            try
            {
                Dictionary<ObjectPath, IDevice1> devices = await GetDeviceDictionaryListAsync(context);
                var deviceInfo = await device.GetAllAsync();
                List<string> paths = new();
                foreach (var item in devices)
                {
                    var info = await item.Value.GetAllAsync();
                    if (info.Address == deviceInfo.Address)
                    {
                        paths.Add(item.Key.ToString());
                    }
                }
                return paths;
            }
            catch (Exception ex)
            {
                log.Debug(ex.Message);
            }
            return new List<string>();
        }

        public static async Task<IDevice1> GetDeviceAsync(ServerContext context, string address)
        {
            Dictionary<ObjectPath, IDevice1> devices = await GetDeviceDictionaryListAsync(context);
            foreach (var item in devices)
            {
                var info = await item.Value.GetAllAsync();
                if (info.Address == address)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public static void PrintProperties(Device1Properties obj)
        {
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(obj);
                log.Debug($"{property.Name}: {value}");
            }
        }

        static async Task<bool> CheckDeviceByAddress(ServerContext context, string address)
        {
            var devices = await GetDeviceListAsync(context);
            foreach (var device in devices)
            {
                var currentDeviceAddress = await device.GetAddressAsync();
                var paired = await device.GetPairedAsync();
                if (currentDeviceAddress == address && paired)
                {
                    return true;
                }
            }
            return false;
        }

        public static IDevice1 GetDeviceByObject(ServerContext context, string objectPath)
        {
            return context.Connection.CreateProxy<IDevice1>(BlueZConstants.BASE_PATH, objectPath);
        }

    }
}