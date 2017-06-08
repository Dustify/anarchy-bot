using System;
using System.Xml.Serialization;


namespace AoLib.Utils
{
    [XmlRoot("character", Namespace="", IsNullable=false)]
    public class WhoisResult
    {
        [XmlElement("name")]
        public WhoisResult_Name Name;
        [XmlElement("basic_stats")]
        public WhoisResult_Stats Stats;
        [XmlElement("pictureurl")]
        public string PictureURL;
        [XmlElement("smallpictureurl")]
        public string SmallPictureURL;
        [XmlElement("organization_membership")]
        public WhoisResult_Organization Organization;
        [XmlElement("last_updated")]
        public string LastUpdated;

        public bool Success
        {
            get
            {
                return (Name != null && Stats != null && Stats.Level > 0 && !string.IsNullOrEmpty(Stats.Profession));
            }
        }

        public bool InOrganization
        {
            get
            {
                return (this.Organization != null && this.Organization.Name != null && this.Organization.Name != string.Empty);
            }
        }
    }

    public class WhoisResult_Name
    {
        [XmlElement("firstname")]
        public string Firstname;
        [XmlElement("nick")]
        public string Nickname;
        [XmlElement("lastname")]
        public string Lastname;

        public override string ToString()
        {
            bool isnamevalid = false;

            string result = "";
            if (!string.IsNullOrEmpty(Firstname))
            {
                result += this.Firstname + " ";
                isnamevalid = true;
            }

            result += string.Format("\"{0}\"", this.Nickname);

            if (!string.IsNullOrEmpty(Lastname))
            {

                result += " " + this.Lastname;
                isnamevalid = true;
            }

            if (isnamevalid == false)result = this.Nickname;

            return result;
        }
    }

    public class WhoisResult_Stats
    {
        [XmlElement("level", Type = typeof(Int32))]
        public Int32 Level;
        [XmlElement("breed")]
        public string Breed;
        [XmlElement("gender")]
        public string Gender;
        [XmlElement("faction")]
        public string Faction;
        [XmlElement("profession")]
        public string Profession;
        [XmlElement("profession_title")]
        public string Title;
        [XmlElement("defender_rank")]
        public string DefenderRank;
        [XmlElement("defender_rank_id", Type = typeof(Int32))]
        public Int32 DefenderLevel = 0;
    }

    public class WhoisResult_Organization
    {
        [XmlElement("organization_id", Type = typeof(Int32))]
        public Int32 ID = 0;
        [XmlElement("organization_name")]
        public string Name;
        [XmlElement("rank")]
        public string Rank;
        [XmlElement("rank_id", Type = typeof(Int32))]
        public Int32 RankID = 0;

        public override string ToString()
        {
            return this.Rank + " of " + this.Name;
        }
    }
}
