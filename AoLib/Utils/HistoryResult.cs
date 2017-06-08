using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace AoLib.Utils
{
    [XmlRoot("character", Namespace = "", IsNullable = false)]
    public class HistoryResult_Root
    {
        [XmlElement("history")]
        public HistoryResult History;
    }

    public class HistoryResult
    {
        [XmlElement("entry")]
        public HistoryResult_Entry[] Items;
    }

    public class HistoryResult_Entry
    {
        [XmlAttribute("date")]
        public string Date;
        [XmlAttribute("level")]
        public string _level;
        public Int32 Level { get { try { return Convert.ToInt32(this._level); } catch { return 0; } } }
        [XmlAttribute("ailevel")]
        public string _defenderLevel;
        public Int32 DefenderLevel { get { try { return Convert.ToInt32(this._defenderLevel); } catch { return 0; } } }
        [XmlAttribute("faction")]
        public string Faction;
        [XmlAttribute("guild")]
        public string Organization;
        [XmlAttribute("rank")]
        public string Rank;
    }
}
