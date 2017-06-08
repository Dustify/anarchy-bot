using System;
using System.Xml.Serialization;
using AoLib.Net;

namespace AoLib.Utils
{
    [XmlRoot("aostats")]
    public class ServerStatusResult
    {
        [XmlElement("dimension")]
        public ServerStatusResult_Dimension[] Dimensions;
        [XmlAttribute("version")]
        public string Version;
        [XmlAttribute("timestamp")]
        public string Date;

        public ServerStatusResult_Dimension GetDimension(Server server)
        {
            foreach (ServerStatusResult_Dimension dim in this.Dimensions)
                if (dim.Name.Replace(" ","").ToLower() == server.ToString().ToLower())
                    return dim;
            return null;
        }
    }

    public class ServerStatusResult_Dimension
    {
        [XmlAttribute("name")]
        public string ID;
        [XmlAttribute("display-name")]
        public string Name;

        [XmlAttribute("loadmax")]
        public string _loadmax;
        public Int32 Loadmax { get { try { return Convert.ToInt32(this._loadmax); } catch { return 0; } } }
        [XmlAttribute("locked")]
        public string _locked;
        public bool Locked
        {
            get
            {
                if (this._locked == "1")
                    return true;
                else
                    return false;
            }
        }
        [XmlAttribute("players")]
        public string _players;
        public Double Players
        {
            get
            {
                if (this._players.Length < 1)
                    return 0;
                string players = this._players.Substring(0, this._players.Length - 1);
                try
                {
                    return Double.Parse(players);
                }
                catch { }
                return 0;
            }
        }

        [XmlElement("servermanager")]
        public ServerStatusResult_Status ServerManager;
        [XmlElement("clientmanager")]
        public ServerStatusResult_Status ClientManager;
        [XmlElement("chatserver")]
        public ServerStatusResult_Status ChatServer;
        [XmlElement("playfield")]
        public ServerStatusResult_Playfield[] Playfields;

       /* [XmlElement("distribution")]
        public ServerStatusResult_Distribution Distribution;
        */
        public ServerStatusResult_Playfield GetPlayfield(Int32 id)
        {
            foreach (ServerStatusResult_Playfield pf in this.Playfields)
                if (pf.ID == id)
                    return pf;
            return null;
        }
        public ServerStatusResult_Playfield GetPlayfield(string name)
        {
            foreach (ServerStatusResult_Playfield pf in this.Playfields)
                if (pf.Name.ToLower() == name.ToLower())
                    return pf;
            return null;
        }
    }

    public class ServerStatusResult_Status
    {
        [XmlAttribute("status")]
        public string _status;
        public bool Online
        {
            get
            {
                if (this._status == "1")
                    return true;
                else
                    return false;
            }
        }
    }

    public class ServerStatusResult_Playfield
    {
        [XmlAttribute("id")]
        public string _id;
        public Int32 ID { get { try { return Convert.ToInt32(this._id); } catch { return 0; } } }
        [XmlAttribute("name")]
        public string Name;
        [XmlAttribute("status")]
        public string _status;
        public PlayfieldStatus Status
        {
            get
            {
                switch (this._status)
                {
                    case "1":
                        return PlayfieldStatus.Online;
                    case "2":
                        return PlayfieldStatus.Inactive;
                    default:
                        return PlayfieldStatus.Offline;
                }
            }
        }
        [XmlAttribute("load")]
        public string _load;
        public Int32 Load { get { try { return Convert.ToInt32(this._load); } catch { return 0; } } }
        [XmlAttribute("players")]
        public string _players;
        public Double Players
        {
            get
            {
                if (this._players.Length < 1)
                    return 0;
                string players = this._players.Substring(0, this._players.Length - 1);
                //players = players.Replace(".", ",");
                try
                {
                    return Double.Parse(players);
                }
                catch { }
                return 0;
            }
        }
    }

    public enum PlayfieldStatus
    {
        Offline,
        Online,
        Inactive
    }

   /* public class ServerStatusResult_Distribution
    {
        [XmlAttribute("type")]
        public string Type;
        [XmlAttribute("subtype")]
        public string SubType;

        [XmlElement("omni")]
        public ServerStatusResult_DistributionFaction Omni;
        [XmlElement("neutral")]
        public ServerStatusResult_DistributionFaction Neutral;
        [XmlElement("clan")]
        public ServerStatusResult_DistributionFaction Clan;
    }
    */
    /*
    public class ServerStatusResult_DistributionFaction
    {
        [XmlAttribute("percent")]
        public string _percent;
        public Double Percent
        {
            get
            {
                string percent = this._percent.Replace(".", ",");
                try
                {
                    return Double.Parse(percent);
                }
                catch { }
                return 0;
            }
        }
  
        public override string ToString()
        {
            return this._percent.ToString();
        }
    } */
}
