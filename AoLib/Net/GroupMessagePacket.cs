using System;
using System.Net;

namespace AoLib.Net
{
	/// <summary>
	/// Class for handling group messages.
	/// </summary>
	internal class GroupMessagePacket : Packet
	{
		/// <summary>
		/// constructor for incomming packets
		/// </summary>
		/// <param name="type">the Packet.Type of the packet</param>
		/// <param name="data">the byte array from the socket</param>
		internal GroupMessagePacket(Packet.Type type, byte[] data): base(type, data) {}

		/// <summary>
		/// constructor for outgoing packets
		/// </summary>
		/// <param name="ChannelID">5-byte channel id</param>
		/// <param name="Text">the text of the message.  This can be html formatted.</param>
		/// <param name="VoiceCommand">the voice command, if any</param>
		internal GroupMessagePacket(BigInteger ChannelID, String Text, VoiceBlob VoiceCommand): 
			base(Packet.Type.GROUP_MESSAGE)
		{
			this.AddData(ChannelID);
			this.AddData(new AoString(Text));

			if (VoiceCommand == null)
				this.AddData(new AoString("\0"));
			else
				this.AddData(VoiceCommand);
		}

		/// <summary>
		/// Maps the byte array into readable data
		/// </summary>
		/// <param name="data">the byte array</param>
		override protected void BytesToData(byte[] data)
		{
			if (data == null || data.Length < 4) { return; }

			int offset = 0;
			this.AddData(popChannelID(ref data, ref offset));
			this.AddData(popUnsignedInteger(ref data, ref offset));
			this.AddData(popString(ref data, ref offset).ToString());

			String blob = popString(ref data, ref offset).ToString();
			if (blob != String.Empty && blob != "\0" && blob.Trim().Length > 0)
				this.AddData(new VoiceBlob(blob, false));
		}

		/// <summary>
		/// 5-byte channel id
		/// </summary>
		internal BigInteger ChannelID
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 1)
				{
					return 0;
				}
                Object o = this.Data[0];
                return (BigInteger)o;
			}
		}

		/// <summary>
		/// buddy id of the sender
		/// </summary>
		internal UInt32 SenderID
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

		/// <summary>
		/// message contents containing text and click links
		/// </summary>
        internal string Message
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 3 || this.Data[2] == null)
				{
					return null;
				}
                Object o = this.Data[2];
                return (string)o;
			}
		}

		/// <summary>
		/// voice blob of the message, if any
		/// </summary>
		internal VoiceBlob VoiceCommand
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 4 || this.Data[3] == null)
				{
					return null;
				}
                Object o = this.Data[3];
                return (VoiceBlob)o;
			}
		}
	}

	/// <summary>
	/// Class for holding event args for AO Chat group message events.
	/// </summary>
	public class ChannelMessageEventArgs : EventArgs
	{
		private readonly BigInteger _channelID = 0;
		private readonly UInt32 _senderID = 0;
        private readonly string _message = null;
		private readonly VoiceBlob _blob = null;
        private ChannelType _type = ChannelType.Unknown;

		// Constructor
        public ChannelMessageEventArgs(BigInteger channelID, UInt32 senderID, string message, VoiceBlob voiceCommand)
		{
			this._channelID = channelID;
			this._senderID = senderID;
			this._message = message;
			this._blob = voiceCommand;
		}

		// Properties
        public BigInteger ChannelID { get { return this._channelID; } }
        public UInt32 SenderID { get { return this._senderID; } }
        public string Message { get { return this._message; } }
        public VoiceBlob VoiceCommand { get { return this._blob; } }
        public ChannelType Type { get { return this._type; } set { this._type = value; } }
	}
}
