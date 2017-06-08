using System;
using System.Text;
using System.Globalization;
using AoLib.Net;

namespace AoLib.Utils
{
    public enum FormatStyle
    {
        Compact,
        Small,
        Medium,
        Large
    }
    public static class Format
    {
        public static string Whois(string user, Server server, FormatStyle style)
        {
            return Whois(user, server, style, false);
        }

        public static string Whois(string user, Server server, FormatStyle style, Boolean fullname)
        {
            WhoisResult whois = XML.GetWhois(user, server);
            if (whois == null || !whois.Success)
                return UppercaseFirst(user);
            return Whois(whois, style);
        }

        public static string Whois(WhoisResult whois, FormatStyle style)
        {
            return Whois(whois, style, false);
        }

        public static string Whois(WhoisResult whois, FormatStyle style, Boolean fullname)
        {
            if (whois == null || !whois.Success)
                return null;

            string result = whois.Name.Nickname;

            if (fullname == true)
            {
                result = whois.Name.ToString();
            }
           
            switch (style)
            {
                case FormatStyle.Small:
                    result += string.Format(" (L {0}/{1} {2})", whois.Stats.Level, whois.Stats.DefenderLevel, whois.Stats.Profession);
                    break;
                case FormatStyle.Compact:
                    result += string.Format(" (L {0}/{1} {2} {3})", whois.Stats.Level, whois.Stats.DefenderLevel, whois.Stats.Faction, whois.Stats.Profession);
                    break;
                case FormatStyle.Medium:
                    result += string.Format(" (Level {0}/{1} {2} {3})", whois.Stats.Level, whois.Stats.DefenderLevel, whois.Stats.Faction, whois.Stats.Profession);
                    if (whois.InOrganization)
                        result += string.Format(" {0} of {1}", whois.Organization.Rank, whois.Organization.Name);
                    break;
                case FormatStyle.Large:
                    result += string.Format(" (Level {0} / Defender Rank {1}) {2} {3} {4}", whois.Stats.Level, whois.Stats.DefenderLevel, whois.Stats.Faction, whois.Stats.Breed, whois.Stats.Profession);
                    if (whois.InOrganization)
                        result += string.Format(", {0} of {1}", whois.Organization.Rank, whois.Organization.Name);
                    break;
            }

            return result;
        }

        public static string Date(long timestamp, FormatStyle style) { return Date(TimeStamp.ToDateTime(timestamp), style); }
        public static string Date(DateTime date, FormatStyle style)
        {
            DateTimeFormatInfo dtfi = new CultureInfo("en-US", false).DateTimeFormat;
            switch (style)
            {
                case FormatStyle.Compact:
                    return date.ToString("dd/MM/yyyy", dtfi);
                case FormatStyle.Small:
                    return date.ToString("dd/MM", dtfi);               
                case FormatStyle.Large:
                    return date.ToString("dddd, MMMM d, yyyy", dtfi);
                default:
                    return date.ToString("d MMMM yyyy", dtfi);
            }
        }

        public static string DateTime(long timestamp, FormatStyle style) { return DateTime(TimeStamp.ToDateTime(timestamp), style); }
        public static string DateTime(DateTime date, FormatStyle style)
        {
            return Date(date, style) + ", " + Time(date, style);
        }

        public static string Time(TimeSpan time, FormatStyle style)
        {
            switch (style)
            {
                case FormatStyle.Small:
                case FormatStyle.Compact:
                    return string.Format("{0:00}:{1:00}", Math.Floor(time.TotalHours), time.Minutes);
                case FormatStyle.Large:
                    return string.Format("{0} hours, {1} minutes and {2} seconds", Math.Floor(time.TotalHours), time.Minutes, time.Seconds);
                default:
                    return string.Format("{0:00}:{1:00}:{2:00}", Math.Floor(time.TotalHours), time.Minutes, time.Seconds);
            }
        }

        public static string Time(DateTime time, FormatStyle style)
        {
            switch (style)
            {
                case FormatStyle.Small:
                case FormatStyle.Compact:
                    return string.Format("{0:00}:{1:00}", time.Hour, time.Minute);
                default:
                    return string.Format("{0:00}:{1:00}:{2:00}", time.Hour, time.Minute, time.Second);
            }
        }

        public static string UppercaseFirst(string text)
        {
            if (text == null)
                return string.Empty;

            if (text.Length < 1)
                return text;

            char[] charArray = text.ToCharArray();
            StringBuilder builder = new StringBuilder();
            bool first = true;
            for (int i = 0; i < charArray.Length; i++)
            {
                builder.Append(first ? charArray[i].ToString().ToUpper() : charArray[i].ToString().ToLower());
                first = true;
                if (charArray[i] >= 65 && charArray[i] <= 90) // upper case chars
                    first = false;
                if (charArray[i] >= 97 && charArray[i] <= 122) // lower case chars
                    first = false;
                if (charArray[i] >= 48 && charArray[i] <= 57) // numbers
                    first = false;
            }
            return builder.ToString();
        }
    }
}
