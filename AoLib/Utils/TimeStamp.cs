using System;
using System.Collections.Generic;
using System.Text;

namespace AoLib.Utils
{
    public static class TimeStamp
    {
        /// <summary>
        /// Converts a DateTime to UnixTimeStamp. Assumes the time it has been given is GMT
        /// </summary>
        /// <param name="time"></param>
        /// <returns>UnixTimeStamp</returns>
        public static Int64 FromDateTime(DateTime time)
        {
            time = time.ToUniversalTime();
            TimeSpan span = time - new DateTime(1970, 1, 1);
            return Convert.ToInt64(span.TotalSeconds);
        }

        /// <summary>
        /// Converts a UnixTimeStamp to DateTime.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(Int64 time)
        {
            DateTime datetime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            datetime = datetime.AddSeconds(Convert.ToDouble(time));
            return datetime;
        }

        /// <summary>
        /// Returns the current time in UnixTimeStamp format as GMT
        /// </summary>
        public static Int64 Now { get { return FromDateTime(DateTime.Now); } }
    }
}
