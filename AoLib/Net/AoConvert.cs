using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace AoLib.Net
{
    public static class AoConvert
    {
        public static UInt32 ToUInt32(Int32 value)
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
        }

        public static Int32 ToInt32(UInt32 value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

        public static UInt32 HostToNetworkOrder(UInt32 value)
        {
            return ToUInt32(IPAddress.HostToNetworkOrder(ToInt32(value)));
        }

        public static UInt32 NetworkToHostOrder(UInt32 value)
        {
            return ToUInt32(IPAddress.NetworkToHostOrder(ToInt32(value)));
        }
    }
}
