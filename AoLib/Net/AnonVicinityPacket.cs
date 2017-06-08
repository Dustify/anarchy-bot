using System;
using System.Net;

namespace AoLib.Net
{
	/// <summary>
	/// Class for anonymous vicinity packets
	/// </summary>
	internal class AnonVicinityPacket : Packet
	{
		/// <summary>
		/// Constructor for incomming packets
		/// </summary>
		/// <param name="type">the Packet.Type of this packet</param>
		/// <param name="data">the byte array containing socket data</param>
		internal AnonVicinityPacket(Packet.Type type, byte[] data): base(type, data) {}

		/// <summary>
		/// Constructor for outgoing packets
		/// </summary>
		/// <param name="FormattedText">The contents of the message.  Can contain click links and color formatting in HTML.</param>
		/// <param name="VoiceCommand">The Voice command to send, if any.  Can be null.</param>
		internal AnonVicinityPacket(String str, String FormattedText, VoiceBlob VoiceCommand): 
			base(Packet.Type.MSG_ANONVICINITY)
		{
			this.AddData(new AoString("\0"));
			this.AddData(new AoString(FormattedText));
			this.AddData(VoiceCommand);
		}
		/// <summary>
		/// Maps the byte array into readable data
		/// </summary>
		/// <param name="data">the byte array</param>
		override protected void BytesToData(byte[] data)
		{
			if (data == null || data.Length < 6) { return; }

			int offset = 0;
			this.AddData(popString(ref data, ref offset));
			this.AddData(popString(ref data, ref offset).ToString());

			String blob = popString(ref data, ref offset).ToString();
			if (blob != String.Empty && blob.Trim().Length > 0)
				this.AddData(new VoiceBlob(blob, false));
		}

		/// <summary>
		/// an unknown string value
		/// </summary>
		internal String UnknownString
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 1)
				{
					return null;
				}
                Object o = this.Data[0];
                return o.ToString();
			}
		}

		/// <summary>
		/// the text of the message containing text and click links.
		/// </summary>
		internal string Message
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 2)
				{
					return null;
				}
                Object o = this.Data[1];
                return (string)o;
			}
		}

		/// <summary>
		/// The voice blob, if any.
		/// </summary>
		internal VoiceBlob VoiceCommand
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 3)
				{
					return null;
				}
                Object o = this.Data[2];
                return (VoiceBlob)o;
			}
		}
		
	}

	/// <summary>
	/// Class for holding event args for anonymous vicinity messages.
	/// </summary>
	public class AnonVicinityEventArgs : EventArgs
	{
		private readonly String _str = null;
        private readonly string _msg = null;
		private readonly VoiceBlob _blob = null;

		/// <summary>
		/// The event argument constructor
		/// </summary>
		/// <param name="str">an unknown string</param>
		/// <param name="Text">the text of the message</param>
		/// <param name="Blob">the blob of the message, if any</param>
        public AnonVicinityEventArgs(String str, string Message, VoiceBlob VoiceCommand)
		{
			this._str = str;
			this._msg = Message;
			this._blob = VoiceCommand;
		}

		/// <summary>
		/// An unknown string
		/// </summary>
		public String	UnknownString	{	get	{ return this._str;		}	}
		/// <summary>
		/// The text of the message containing text and click links.
		/// </summary>
        public string Message { get { return this._msg; } }
		/// <summary>
		/// the voice blob of the message, if any
		/// </summary>
		public VoiceBlob	VoiceCommand			{	get	{ return this._blob;	}	}
	}
}
