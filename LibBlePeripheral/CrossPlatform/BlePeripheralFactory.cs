using LibBlePeripheral.LinuxBluezServer;
using LibBlePeripheral.WindowsBleServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibBlePeripheral.CrossPlatform
{
    public class BlePeripheralFactory
    {
        public IBlePeripheral GetDriver()
        {

            if (OperatingSystem.IsLinux() == true)
            {
                return new TestLinux();
            }
            else if (OperatingSystem.IsWindows() == true)
            {
                return new TestWin();
            }
            else
            {
                return null;
            }

        }
    }
}
