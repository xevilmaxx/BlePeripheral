namespace LibBlePeripheral.CrossPlatform.MaxStruct
{
    public class MaxBleDevice
    {
        /// <summary>
        /// Will determine the name with which device can be found during scanning
        /// <para/>
        /// Works on Linux, but on Windows you would need to rename Bluetooth adapter manually
        /// <para/>
        /// Please look into Notes.txt on how to do it under windows section
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// All possible GATT Server Services
        /// </summary>
        public List<MaxGattService> Services { get; set; }

        /// <summary>
        /// Will tell us if object composed has no big issues
        /// </summary>
        /// <returns></returns>
        public bool IsStructOk()
        {
            bool result = true;

            //mainly we need to not have duplicated UUIDs
            List<string> allGuids = new List<string>();
            foreach (var service in Services)
            {
                if(allGuids.Contains(service.UUID) == false)
                {

                    allGuids.Add(service.UUID);

                    foreach(var charcteristic in service.Characteristics)
                    {
                        if (allGuids.Contains(charcteristic.UUID) == false)
                        {
                            allGuids.Add(charcteristic.UUID);
                        }
                        else
                        {
                            result = false;
                            break;
                        }

                        if(charcteristic.Descriptor != null)
                        {
                            if (allGuids.Contains(charcteristic.Descriptor.UUID) == false)
                            {
                                allGuids.Add(charcteristic.Descriptor.UUID);
                            }
                            else
                            {
                                result = false;
                                break;
                            }
                        }
                    }

                }
                else
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

    }
}
