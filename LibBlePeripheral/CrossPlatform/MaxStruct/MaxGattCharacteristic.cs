namespace LibBlePeripheral.CrossPlatform.MaxStruct
{
    public class MaxGattCharacteristic
    {
        /// <summary>
        /// Unique identifier of the service
        /// <para/>
        /// Values sould be expressed like: "00000000-0000-0000-0000-000000000001"
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// Characteristic Type: Readable, Writable, ...
        /// </summary>
        public List<MaxCharactPropType> Flags { get; set; }

        /// <summary>
        /// [OPTIONAL] Further Description, works only on Linux
        /// </summary>
        public MaxGattDescriptor Descriptor { get; set; }

        /// <summary>
        /// Something additional to do On external Read occured (someone had readed on us)
        /// <para/>
        /// You can specify it in order to return some dynamic/composed data (like humidity/temperature/...)
        /// </summary>
        public Func<byte[]> OnReadCaptured { get; set; }

        /// <summary>
        /// Something additional to do On external Write occured (someone had written on us)
        /// </summary>
        public Action<byte[]> OnWriteCaptured { get; set; }

    }
}
