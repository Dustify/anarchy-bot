using System;
using System.Text;

namespace AoLib.Mdb
{
    public class DescrambledArgument
    {
        private DescrambledArgumentType _type;
        private String _text;
        private Int32 _integer;
        private UInt32 _unsignedInteger;
        private Single _float;
        private Int32 _categoryID;
        private Int32 _entryID;
        private DescrambledMessage _recursive;
        private String _message;

        public DescrambledArgumentType Type { get { return this._type; } }
        public String Text { get { return this._text; } }
        public Int32 Integer { get { return this._integer; } }
        public UInt32 UnsignedInteger { get { return this._unsignedInteger; } }
        public Single Float { get { return this._float; } }
        public Int32 CategoryID { get { return this._categoryID; } }
        public Int32 EntryID { get { return this._entryID; } }
        public DescrambledMessage Recursive { get { return this._recursive; } }
        public String Message { get { return this._message; } }

        public DescrambledArgument(String text)
        {
            this._text = text;
            this._message = text;
            this._type = DescrambledArgumentType.Text;
        }

        public DescrambledArgument(Int32 integer)
        {
            this._integer = integer;
            this._message = integer.ToString();
            this._type = DescrambledArgumentType.Integer;
        }

        public DescrambledArgument(UInt32 unsignedInteger)
        {
            this._unsignedInteger = unsignedInteger;
            this._message = unsignedInteger.ToString();
            this._type = DescrambledArgumentType.UnsignedInteger;
        }

        public DescrambledArgument(Single single)
        {
            this._float = single;
            this._message = single.ToString();
            this._type = DescrambledArgumentType.Float;
        }

        public DescrambledArgument(Int32 categoryID, Int32 entryID, String message)
        {
            this._categoryID = categoryID;
            this._entryID = entryID;
            this._message = message;
            this._type = DescrambledArgumentType.Reference;
        }

        public DescrambledArgument(DescrambledMessage aoDescrambledMessage)
        {
            this._recursive = aoDescrambledMessage;
            this._message = aoDescrambledMessage.Message;
            this._type = DescrambledArgumentType.Recursive;
            this._categoryID = aoDescrambledMessage.CategoryID;
            this._entryID = aoDescrambledMessage.EntryID;
        }

        public override string ToString()
        {
            return this.Message;
        }
    }
}
