using LibBlePeripheral.LinuxBluezServer.Constants;
using LibBlePeripheral.LinuxBluezServer.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace LibBlePeripheral.LinuxBluezServer.Core
{
    public abstract class PropertiesBase<TV>
    {
        protected readonly PropertiesBase<TV> PropertiesBaseInstance;
        protected readonly TV Properties;

        protected PropertiesBase(ObjectPath objectPath, TV properties)
        {
            PropertiesBaseInstance = this;
            ObjectPath = objectPath;
            Properties = properties;
        }

        public ObjectPath ObjectPath { get; }

        public Task<object> GetAsync(string prop)
        {
            return Task.FromResult(Properties.ReadProperty(prop));
        }

        public Task<T> GetAsync<T>(string prop)
        {
            return Task.FromResult(Properties.ReadProperty<T>(prop));
        }

        public Task<TV> GetAllAsync()
        {
            return Task.FromResult(Properties);
        }

        public Task SetAsync(string prop, object val)
        {
            OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(prop, val));
            return Properties.SetProperty(prop, val);
        }

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
        {
            return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
        }

        public void NotifyValueChange(byte[] data)
        {
            SetAsync("Value", data);
        }
        public async void SetConnectionInterval()
        {
            var propertiesDict = new Dictionary<string, object>
            {
                ["MinConnectionInterval"] = 10,
                ["MaxConnectionInterval"] = 20
            };
            await SetAsync(BlueZConstants.GATT_CHARACTERISTIC_PROP, propertiesDict);
        }

        public event Action<PropertyChanges> OnPropertiesChanged;
    }
}
