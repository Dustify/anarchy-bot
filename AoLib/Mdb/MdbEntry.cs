using System;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AoLib.Mdb
{
    public class MdbEntry
    {
        private Int32 _entryID;
        private Int32 _offset;
        private string _message;

        [XmlAttribute("ID")]
        public Int32 EntryID { get { return this._entryID; } set { this._entryID = value; } }
        [XmlIgnore()]
        public Int32 Offset { get { return this._offset; } set { this._offset = value; } }
        [XmlAttribute("Message")]
        public string Message { get { return this._message; } set { this._message = value; } }

        public MdbEntry() { }
        public MdbEntry(Int32 entryID, Int32 offset, string message)
        {
            this._entryID = entryID;
            this._offset = offset;
            this._message = message;
        }

        public override string ToString()
        {
            return String.Format("[{0}] {1}", this._entryID, this._message);
        }
    }
}