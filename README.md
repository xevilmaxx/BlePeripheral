# What this project is:
Its an attempt to create a simple cross-platform library which works on Windows/Linux without necessity of creating special types of projects.
The aim is to have as simpliest possible init way in order to expose on both platforms GATT Service and Read/Write characteristics.

# Usage Sample

**Basic Struct:**

    var maxStruct = new MaxStructBuilder()
    .BuildTestV3_ExternalActions(
        (data) =>
        {
            Console.WriteLine("ììììì ----->" + Encoding.ASCII.GetString(data));
        },
        () =>
        {
            //var result = CoreLiteGate.OpenForwardBar(new IPC.SHARED.NoArgs() { });
            return Encoding.ASCII.GetBytes($"okkkkkk");
        }
    );

**Than in order to expose it simply do:**

    var ble = new BlePeripheralFactory()
	    .GetDriver()
	    .SetMaxBleDevice(maxStruct)
	    .Expose();

**Advanced struct usage:**

        var maxStruct = new MaxBleDevice()
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

You can attach something to execute (labda [which may use your local variables]) when someone tries to **Write** certain characteristic and you can define dynamic data to return in case of **Read** is received.

# Platfrom Notes:

 - Definition of device Name
	 - **Linux**
		 - Works perfectly
	 - **Windows**
		 - You need set it manually from windows settings (guide is provided in Notes.txt under LibBlePeripheral/WindowsBleServer )

# Trick used to make Windows work in normal ConsoleApp:
Extracted DLLs from nuget package: **Microsoft.Windows.SDK.NET.Ref**, and referenced the oldes version to gain as much compatibility as possible.
You are free to change the SDK reference version, just add reference to different local DLLs which are already included in LibBlePeripheral project.


# Tested On:

 - **Windows 11**
 - **Raspberry 3B+** (which means it should work without problems on **RPI 4**)
	 - Basically will work on any linux that supports:
		 - **BlueZ**
		 - **Dbus**

# Extras:
Included a **MAUI** implementation of client in order to play around with exposed server.
Best pages for Test:

 - /
 - /**bleactonvicinity**
	 - this one will basically attempt to call some characteristic on exposed server as soon as its discovered, with some timeouts (***eventually you will need to change DeviceName you are scanning for if its different for you***)

# Frameworks Used:

 - **NET CORE - NET8.0**
 - Pure **C#**
 - Basic **Console App** for Server (**Peripheral**)
 - **MAUI + Blazor** for Mobile Device (**Central**)

# Reminders for me:
Not yet exposable as nuget package as need provide universal way in order to be able to attach a generic logger and avoid dependency from Nlog (expecially for those who use different version of same library).
