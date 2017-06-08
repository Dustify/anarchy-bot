using System;
using System.Net;

namespace AoLib.Net
{
	/// <summary>
	/// Summary description for SystemMessagePacket.
	/// </summary>
	public class SystemMessagePacket : Packet
	{
		/// <summary>
		/// Constructor for incomming packets
		/// </summary>
		/// <param name="type">the Packet.Type of this packet</param>
		/// <param name="data">the byte array containing socket data</param>
		internal SystemMessagePacket(Packet.Type type, byte[] data): base(type, data) {}

		/// <summary>
		/// Maps the byte array into readable data
		/// </summary>
		/// <param name="data">the byte array</param>
		override protected void BytesToData(byte[] data)
		{
			if (data == null || data.Length < 6) { return; }

			int offset = 0;
			
			this.AddData(popUnsignedInteger(ref data, ref offset));
			this.AddData(popUnsignedInteger(ref data, ref offset));
			this.AddData(popUnsignedInteger(ref data, ref offset));
			String blob = popString(ref data, ref offset).ToString();
			if (blob != String.Empty && blob.Trim().Length > 0)
				this.AddData(new BlobArgs(blob));
		}

		internal UInt32 ClientID
		{
			get 
			{
				if (this.Data == null || this.Data.Count < 1)
				{
					return 0;
				}
                Object o = this.Data[0];
                return (UInt32)o;
			}
		}

        internal UInt32 WindowID
		{
			get 
			{
				if (this.Data == null || this.Data.Count < 2)
				{
					return 0;
				}
                Object o = this.Data[1];
                return (UInt32)o;
			}
		}

        internal UInt32 MessageID
		{
			get 
			{
				if (this.Data == null || this.Data.Count < 3)
				{
					return 0;
				}
                Object o = this.Data[2];
                return (UInt32)o;
			}
		}

		internal String Message
		{
			get
			{
                /*if (this.MessageID > 0)
                {
                    SysMsgDataset ds = new SysMsgDataset();
                    System.Reflection.Assembly thisExe = System.Reflection.Assembly.GetExecutingAssembly();
                    System.IO.Stream xml = thisExe.GetManifestResourceStream("Ao.Net.SystemMessages.xml");

                    if (xml == null)
                        return String.Empty;

                    ds.ReadXml(xml);
                    if (ds != null)
                    {
                        SysMsgDataset.SysMsgValuesRow row = 
                            ds.SysMsgValues.FindBymessageId(this.MessageID);

                        if (row != null)
                            return String.Format(row.message, this.Blob.Args);
                    }
                }*/
				return String.Empty;
			}
		}

		internal BlobArgs Blob
		{
			get
			{
				if (this.Data == null || this.Data.Count < 4)
				{
					return null;
				}
                Object o = this.Data[3];
                return (BlobArgs)o;
			}
		}
	}

	/// <summary>
	/// Class for holding event args for system messages.
	/// </summary>
	public class SystemMessagePacketEventArgs : EventArgs
	{
        private readonly UInt32 _client = 0;
        private readonly UInt32 _window = 0;
        private readonly UInt32 _msg = 0;
		private readonly String _msgText = String.Empty;

        public SystemMessagePacketEventArgs(UInt32 ClientID, UInt32 WindowID, UInt32 MessageID, String Message)
		{
			this._client = ClientID;
			this._window = WindowID;
			this._msg = MessageID;
			this._msgText = Message;
		}

        public UInt32 ClientID { get { return this._client; } }
        public UInt32 WindowID { get { return this._window; } }
        public UInt32 MessageID { get { return this._msg; } }
        public String Message { get { return this._msgText; } }
	}
}

