using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace AoLib.Utils
{
    public class AoItem
    {
        public readonly string Name;
        public readonly Int32 LowID;
        public readonly Int32 HighID;
        public readonly Int32 QL;
        public readonly string Raw;

        public AoItem(string name, Int32 lowid, Int32 highid, Int32 ql, string raw)
        {
            this.Name = name;
            this.LowID = lowid;
            this.HighID = highid;
            this.QL = ql;
            this.Raw = raw;
        }

        public override string ToString()
        {
            return string.Format("QL {0} {1}", this.QL, this.Name);
        }

        public string ToLink()
        {
            return HTML.CreateItem(this.Name, this.LowID, this.HighID, this.QL);
        }

        private static Regex Regex;
        public static AoItem[] ParseString(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return new AoItem[0];
            if (AoItem.Regex == null)
                AoItem.Regex = new Regex("<a href=\"itemref://([0-9]+)/([0-9]+)/([0-9]{1,3})\">([^<]+)</a>");

            List<AoItem> items = new List<AoItem>();
            MatchCollection matches = AoItem.Regex.Matches(raw);
            foreach (Match match in matches)
            {
                try
                {
                    string name = match.Groups[4].Value;
                    Int32 lowid = Convert.ToInt32(match.Groups[1].Value);
                    Int32 highid = Convert.ToInt32(match.Groups[2].Value);
                    Int32 ql = Convert.ToInt32(match.Groups[3].Value);
                    items.Add(new AoItem(name, lowid, highid, ql, match.Groups[0].Value));
                }
                catch { }
            }
            return items.ToArray();
        }
    }
}
