using System;
using System.Collections.Generic;
using System.Text;

namespace AoLib.Mdb
{
    public class DescrambledMessage
    {
        private Int32 _categoryID = 0;
        private Int32 _entryID = 0;
        private List<DescrambledArgument> _arguments = new List<DescrambledArgument>();
        private String _message = null;
        private String _raw = null;

        public Int32 CategoryID { get { return this._categoryID; } }
        public Int32 EntryID { get { return this._entryID; } }
        public DescrambledArgument[] Arguments { get { return this._arguments.ToArray(); } }
        public String Message
        {
            get { return this._message; }
            set
            {
                if (this._message == null)
                    this._message = value;
                else
                    throw new Exception("AoDescrambledMessage.Message can only be set once!");
            }
        }
        public String Raw { get { return this._raw; } }

        public DescrambledMessage(Int32 categoryID, Int32 entryID, string raw)
        {
            this._categoryID = categoryID;
            this._entryID = entryID;
            this._raw = raw;
        }

        public void Append(DescrambledArgument argument)
        {
            if (this._categoryID == 0 && this._entryID == 0)
                throw new Exception("Trying to append to an invalid AoDescrambledMessage object! CategoryID and EntryID are both 0!");

            this._arguments.Add(argument);
        }
    }
}
