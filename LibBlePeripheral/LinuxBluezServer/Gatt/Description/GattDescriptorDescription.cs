﻿namespace LibBlePeripheral.LinuxBluezServer.Gatt.Description
{
    public class GattDescriptorDescription
    {
        public byte[] Value { get; set; }
        public string UUID { get; set; }
        public string[] Flags { get; set; }
    }
}