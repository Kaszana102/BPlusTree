using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StukturyBazDanych2
{
    /// <summary>
    /// tuple of record and key
    /// </summary>    
    internal class RecordKey
    {
        public int key;
        public Record record;

        public static int RecordKeysBinarySize = Record.RecordBinarySize + 4;

        public RecordKey(int key, Record record)
        {
            this.key = key;
            this.record = record;
        }

        public override string ToString()
        {
            return "["+key.ToString()+"] " + record;
        }

        public byte[] ToBinary()
        {
            List<byte> otp = new List<byte>();


            foreach (byte Byte in IntConvert.intToByte4(key))
            {
                otp.Add(Byte);
            }

            otp.AddRange(record.ToBinary());

            return otp.ToArray();

        }

        static public RecordKey FromBinary(byte[] src)
        {            

            int key = IntConvert.byte4Toint(src);            

            Record record = Record.FromBinary(src.Skip(4).ToArray());

            return new RecordKey(key,record);
        }

    }
}
