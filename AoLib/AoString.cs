using System;
using System.Text;
using System.Collections;
using System.Net;

namespace AoLib
{
    /// <summary>
    /// Summary description for AoString.
    /// </summary>
    public class AoString
    {
        protected String _str;
        protected short _len;
        protected Encoding enc = Encoding.GetEncoding("utf-8");
        public AoString(byte[] data): this(data, 0) {}
        public AoString(byte[] data, int StartIndex)
        {
            if (data == null || data.Length - StartIndex < 3) { return; }

            this._len = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, StartIndex));
            this._str = this.enc.GetString(data, 2 + StartIndex, this._len);
        }
        public AoString(String str)
        {

            this._str = str;
            if (str == null)
            {
                this._len = 0;
            }
            else
            {
                this._len = (short)str.Length;
            }
        }
        public String Value
        {
            get { return (this._str ?? String.Empty); }
        }
        public short Length
        {
            get { return this._len; }
        }
        public int TotalLength
        {
            get { return this.Length + BitConverter.GetBytes(this._len).Length; }
        }
        override public String ToString()
        {
            StringBuilder ret = new StringBuilder(this.Value);
            return ret.ToString();
        }
        public byte[] GetBytes()
        {
            if (this._str == null)
                return null;
            byte[] value = this.enc.GetBytes(this._str);
            byte[] bytes = new byte[value.Length + 2];
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)value.Length)).CopyTo(bytes, 0);
            value.CopyTo(bytes, 2);
            return bytes;
        }
    }
}
