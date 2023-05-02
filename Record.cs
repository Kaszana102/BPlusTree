using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StukturyBazDanych2
{
    [Serializable()]
    internal class Record
    {
        public static int RecordBinarySize = 5 * 4; //five vals of 4 byte int
        List<int> vals;
        public Record(int val)
        {
            vals = new List<int>();
            for(int i = 0; i < 5; i++)
            {
                vals.Add(val);
            }
        }

        public Record(int[] array)
        {
            vals = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                vals.Add(array[i]);
            }
        }


        public override string ToString()
        {
            string otp="";
            foreach(var v in vals)
            {
                otp += v + " ";
            }

            return otp;
        }


        public byte[] ToBinary()
        {
            List<byte> otp= new List<byte>();

            for (int i = 0; i < 5; i++)
            {
                foreach (byte Byte in IntConvert.intToByte4(vals[i]))
                {
                    otp.Add(Byte);
                }              
            }
            
            return otp.ToArray();
        }

        public static Record FromBinary(byte[] src)
        {
            int[] vals = new int[5]; //vals of record

            for (int i = 0; i < 5; i++)
            {                                
                vals[i] = IntConvert.byte4Toint(src.Skip(4*i).ToArray());                                
            }
            return new Record(vals);
        }
    }
}
