using LibBlePeripheral.CrossPlatform.MaxStruct;
using LibBlePeripheral.LinuxBluezServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibBlePeripheral.CrossPlatform
{
    public interface IBlePeripheral
    {

        public IBlePeripheral SetMaxBleDevice(MaxBleDevice Data);

        public IBlePeripheral Expose();

    }
}
