using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
namespace Sociedade_Serial
{
    class constants
    {
        public int[] baudRates = new int[] { 110, 300, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 };
        public int[] dataBits = new int[] { 5,6,7,8,};
        public string[] stopBits = new string[] {"1", "2", "1.5"};
        public string[] parity = new string[] {Parity.Even.ToString(), Parity.Mark.ToString(), Parity.None.ToString(), Parity.Odd.ToString(), Parity.Space.ToString() };
        public string[] flowControl = new string[] { Handshake.None.ToString(), Handshake.RequestToSend.ToString(), Handshake.RequestToSendXOnXOff.ToString(),Handshake.XOnXOff.ToString() };
    }
    
}
