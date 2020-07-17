using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sociedade_Serial
{
    public class crc
    {
        public string algorithm;
        public UInt64 polynominal, initialValue, finalXorVal;
        public bool inputReflected, resultReflected, lowBitFirst;
        public int from, width;

        public string calcCRC(string str)
        {
            UInt64 crcValue = this.initialValue;
            UInt64 c;
            UInt64 msbMask = (UInt64)(0x1 << (this.width - 1));

            byte[] frame = utils.str2Hex(str);

            UInt64 crcMask;
            switch (this.width)
            {
                case 8: crcMask = 0xFF; break;
                case 16: crcMask = 0xFFFF; break;
                case 32: crcMask = 0xFFFFFFFF; break;
                default: crcMask = UInt64.MaxValue; break;
            }

            foreach (byte b in frame)
            {
                if (this.algorithm != "checksum")
                {
                    if (!this.inputReflected)
                        c = b;
                    else
                        c = reflect(b);

                    crcValue ^= (c << (this.width - 8));
                    crcValue &= crcMask;

                    for (int i = 0; i < 8; i++)
                    {
                        if ((crcValue & msbMask) != 0)
                        {
                            crcValue <<= 1;
                            crcValue ^= this.polynominal;
                        }
                        else
                        {
                            crcValue <<= 1;
                        }
                        crcValue &= crcMask;

                    }


                }else
                {
                    crcValue+= (UInt64)b;
                }

               
            }

            if (this.algorithm != "checksum")
            {
                if (this.resultReflected)
                {
                    crcValue = reflect(crcValue);
                    crcValue &= crcMask;
                }

                crcValue ^= this.finalXorVal;
            }
               
            
            crcValue &= crcMask;


            return displayCRC(crcValue);


        }
        private string displayCRC(UInt64 crcValue)
        {

            switch (this.width)
            {
                case 8: return crcValue.ToString("x2");

                case 16:return utils.getFrameFormat(crcValue.ToString("x4"), this.lowBitFirst);
                case 32: return utils.getFrameFormat(crcValue.ToString("x8"), this.lowBitFirst);
                default: return utils.getFrameFormat(crcValue.ToString("x16"), this.lowBitFirst);
            }

        }
        private UInt64 reflect(byte val)
        {
            UInt16 resByte = 0;

            for (int i =0; i<8;i++)
            {
                if ((val & (1 << i)) != 0)
                {
                    
                    resByte |= (UInt16)(1 << (7 - i));
                }
            }
            return resByte;
        }
        private UInt64 reflect(UInt64 val)
        {
            UInt64 resByte = 0;
            for (int i = 0; i < this.width; i++)
            {
                if ((val & (UInt64)(1 << i)) != 0)
                    resByte |= (UInt64)(1 << (this.width - 1 -i));
            }
            return resByte;
        }
    }
}
