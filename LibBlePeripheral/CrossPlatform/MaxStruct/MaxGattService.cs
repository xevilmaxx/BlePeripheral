namespace LibBlePeripheral.CrossPlatform.MaxStruct
{
    public class MaxGattService
    {
        /// <summary>
        /// Unique identifier of the service
        /// <para/>
        /// Values sould be expressed like: "00000000-0000-0000-0000-000000000001"
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// All possible Characteristics of the service
        /// </summary>
        public List<MaxGattCharacteristic> Characteristics { get; set; }
    }
}
