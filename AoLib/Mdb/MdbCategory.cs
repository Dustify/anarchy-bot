using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AoLib.Mdb
{

    public class MdbCategory
    {
        private Int32 _categoryID;
        private Int32 _offset;
        private List<MdbEntry> _entries;

        [XmlAttribute("ID")]
        public Int32 CategoryID { get { return this._categoryID; } set { this._categoryID = value; } }
        [XmlIgnore()]
        public Int32 Offset { get { return this._offset; } set { this._offset = value; } }
        [XmlElement("Entry")]
        public MdbEntry[] Entries
        {
            get { return this._entries.ToArray(); }
            set
            {
                this._entries = new List<MdbEntry>();
                foreach (MdbEntry entry in value)
                    this._entries.Add(entry);
            }
        }

        public MdbCategory() { }
        public MdbCategory(Int32 categoryID, Int32 offset)
        {
            this._categoryID = categoryID;
            this._offset = offset;
            this._entries = new List<MdbEntry>();
        }

        public bool Add(MdbEntry entry)
        {
            if (entry == null)
                return false;

            if (this.GetEntry(entry.EntryID) != null)
                return false;

            if (this._entries != null)
            {
                lock (this._entries)
                {
                    this._entries.Add(entry);
                    return true;
                }
            }

            return false;

        }

        public MdbEntry GetEntry(Int32 entryID)
        {
            if (this._entries != null)
                lock (this._entries)
                    foreach (MdbEntry entry in this._entries)
                        if (entry.EntryID == entryID)
                            return entry;

            return null;
        }

        public override string ToString()
        {
            return String.Format("{0} ({1} Entries)", this._categoryID, this._entries.Count);
        }
    }
}
