namespace LibBlePeripheral.CrossPlatform.MaxStruct
{
    public enum MaxCharactPropType
    {
        Read = 0,
        Write = 1,

        //For simplicity of API will just assume there is only read/write
        /// <summary>
        /// on windows i cannot choose what i should response!
        /// <para/>
        /// on Linux there is even no difference, so typically there is no big difference between Write/WriteNoResponse
        /// </summary>
        //WriteWithoutResponse = 2
    }
}
