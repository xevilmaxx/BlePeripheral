using LibBlePeripheral.LinuxBluezServer.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace LibBlePeripheral.LinuxBluezServer.Gatt.BlueZModel
{
    [DBusInterface("org.freedesktop.DBus.ObjectManager")]
    internal class GattApplication : IObjectManager
    {
        private readonly IList<GattService> _Services = new List<GattService>();

        public GattApplication(ObjectPath objectPath)
        {
            ObjectPath = objectPath;
        }

        public ObjectPath ObjectPath { get; }

        public async Task<IDisposable> WatchInterfacesAddedAsync(Action<(ObjectPath @object, IDictionary<string, IDictionary<string, object>> interfaces)> handler, Action<Exception> onError = null)
        {
            await Task.Yield();
            return Task.CompletedTask;
        }

        public async Task<IDisposable> WatchInterfacesRemovedAsync(Action<(ObjectPath @object, string[] interfaces)> handler, Action<Exception> onError = null)
        {
            await Task.Yield();
            return Task.CompletedTask;
        }

        public Task<IDictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>> GetManagedObjectsAsync()
        {
            IDictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>> result =
                new Dictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>();
            foreach (var service in _Services)
            {
                result[service.ObjectPath] = service.GetProperties();
                foreach (var characteristic in service.Characteristics)
                {
                    result[characteristic.ObjectPath] = characteristic.GetProperties();
                    foreach (var descriptor in characteristic.Descriptors)
                    {
                        result[descriptor.ObjectPath] = descriptor.GetProperties();
                    }
                }
            }

            return Task.FromResult(result);
        }

        public GattService AddService(GattService1Properties gattService)
        {
            var servicePath = ObjectPath + "/service" + _Services.Count;
            var service = new GattService(servicePath, gattService);
            _Services.Add(service);
            return service;
        }
    }
}