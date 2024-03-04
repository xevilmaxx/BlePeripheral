using LibBlePeripheral.CrossPlatform.MaxStruct;
using System.Text;

namespace LibBlePeripheral.CrossPlatform
{
    public class MaxStructBuilder
    {

        //private byte[] ChangableData;

        public MaxBleDevice BuildTest1Service()
        {
            return new MaxBleDevice()
            {
                Name = "EvilMax",
                Services = new List<MaxGattService>()
                {
                    new MaxGattService()
                    {
                        UUID = "00000000-0000-0000-0000-000000000001",
                        Characteristics = new List<MaxGattCharacteristic>()
                        {
                            new MaxGattCharacteristic()
                            {
                                UUID = "00000000-0000-0000-0000-000000000002",
                                Flags = new List<MaxCharactPropType>() { MaxCharactPropType.Read, MaxCharactPropType.Write },
                                Descriptor = new MaxGattDescriptor()
                                {
                                    UUID = "00000000-0000-0000-0000-000000000003",
                                    StaticValue = new byte[] { (byte)'T' }
                                }
                            }
                        }
                    }
                }
            };
        }

        public MaxBleDevice BuildTest2Services()
        {

            //_ = Task.Run(() => 
            //{
            //    while (true)
            //    {
            //        ChangableData = Encoding.ASCII.GetBytes(new Random().Next().ToString());
            //        Thread.Sleep(10000);
            //    }
            //});

            return new MaxBleDevice()
            {
                Name = "EvilMax",
                Services = new List<MaxGattService>()
                {
                    new MaxGattService()
                    {
                        UUID = "00000000-0000-0000-0000-000000000001",
                        Characteristics = new List<MaxGattCharacteristic>()
                        {
                            new MaxGattCharacteristic()
                            {
                                UUID = "00000000-0000-0000-0000-000000000002",
                                Flags = new List<MaxCharactPropType>() { MaxCharactPropType.Read, MaxCharactPropType.Write },
                                Descriptor = new MaxGattDescriptor()
                                {
                                    UUID = "00000000-0000-0000-0000-000000000021",
                                    StaticValue = Encoding.ASCII.GetBytes("Static Value")
                                },
                                OnWriteCaptured = (receivedData) => 
                                {
                                    Console.WriteLine(Encoding.ASCII.GetString(receivedData)); 
                                }
                            }
                        }
                    },
                    new MaxGattService()
                    {
                        UUID = "00000000-0000-0000-0000-000000000003",
                        Characteristics = new List<MaxGattCharacteristic>()
                        {
                            new MaxGattCharacteristic()
                            {
                                UUID = "00000000-0000-0000-0000-000000000004",
                                Flags = new List<MaxCharactPropType>() { MaxCharactPropType.Read },
                                OnReadCaptured = () =>
                                {
                                    return Encoding.ASCII.GetBytes(new Random().Next().ToString());
                                    //return ChangableData;
                                },
                            }
                        }
                    },
                    new MaxGattService()
                    {
                        UUID = "00000000-0000-0000-0000-000000000005",
                        Characteristics = new List<MaxGattCharacteristic>()
                        {
                            new MaxGattCharacteristic()
                            {
                                UUID = "00000000-0000-0000-0000-000000000006",
                                Flags = new List<MaxCharactPropType>() { MaxCharactPropType.Write },
                                OnWriteCaptured = (receivedData) =>
                                {
                                    Console.WriteLine(Encoding.ASCII.GetString(receivedData));
                                }
                            }
                        }
                    }
                }
            };
        }

        public MaxBleDevice BuildTestV3_ExternalActions(Action<byte[]> WriteCapturedAction, Func<byte[]> ReadCapturedAnswer)
        {
            return new MaxBleDevice()
            {
                Name = "EvilMax",
                Services = new List<MaxGattService>()
                {
                    new MaxGattService()
                    {
                        UUID = "00000000-0000-0000-0000-000000000001",
                        Characteristics = new List<MaxGattCharacteristic>()
                        {
                            new MaxGattCharacteristic()
                            {
                                UUID = "00000000-0000-0000-0000-000000000002",
                                Flags = new List<MaxCharactPropType>() { MaxCharactPropType.Read },
                                OnReadCaptured = ReadCapturedAnswer
                            },
                            new MaxGattCharacteristic()
                            {
                                UUID = "00000000-0000-0000-0000-000000000003",
                                Flags = new List<MaxCharactPropType>() { MaxCharactPropType.Write },
                                OnWriteCaptured = WriteCapturedAction
                            }
                        }
                    }
                }
            };
        }

    }
}
