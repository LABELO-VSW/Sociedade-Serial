using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sociedade_Serial
{
    class utils
    {
        
        public string Ascii2Hex(string ascii_value)
        {
            /*
            * converts ascii text to an string with hex values
            * @param ascii
         */
            string hex = "";
            for(int i = 0; i < ascii_value.Length; i++)
            {
                hex += ascii_value[i] > 57 ? ascii_value[i] - 55 : ascii_value[i] - 48;
                if (i % 2 == 0)
                {
                    hex += " ";
                }
            }

            return hex;
        }
    }
}
