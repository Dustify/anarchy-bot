using System;
using System.Collections.Generic;
using System.Text;

namespace AoLib.Utils
{
    public class Base85
    {
        public static Int32 Decode(string encoded)
        {
            Int32 total = 0;
            char[] chars = encoded.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                Int32 value = (Int32)chars[i] - 33;
                if (value >= 85 || value < 0)
                    return 0;
                total = (total * 85) + value;
            }
            return total;
        }
    }
}
