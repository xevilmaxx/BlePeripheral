namespace LibBlePeripheral.CrossPlatform.MaxStruct
{
    /// <summary>
    /// [OPTIONAL] only for Linux static values (which also may be specified with static string to return on read)
    /// <para/>
    /// Static means there is non need to query in order to get value, its already known after services discovery
    /// </summary>
    public class MaxGattDescriptor
    {

        /// <summary>
        /// Unique identifier of the service
        /// <para/>
        /// Values sould be expressed like: "00000000-0000-0000-0000-000000000001"
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// For eventual static value to return
        /// </summary>
        public byte[] StaticValue { get; set; }

    }
}
