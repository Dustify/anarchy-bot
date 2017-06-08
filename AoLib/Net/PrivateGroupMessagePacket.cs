using System;
using System.Net;

namespace AoLib.Net
{
	/// <summary>
	/// Class for handling private group messages
	/// </summary>
	internal class PrivateGroupMessagePacket : Packet
	{
		/// <summary>
		/// Constructor for incomming packets
		/// </summary>
		/// <param name="type">the Packet.Type of the message</param>
		/// <param name="data">the byte array from the socket</param>
		internal PrivateGroupMessagePacket(Packet.Type type, byte[] data): 
			base(type, data) {}

		/// <summary>
		/// Constructor for outgoing messages
		/// </summary>
		/// <param name="PrivateGroupID">the id of the buddy who is hosting the private group</param>
		/// <param name="FormattedText">The contents of the message.  Can contain click links and color formatting in HTML.</param>
		/// <param name="VoiceCommand">The voice command to send, if any.  Can be null.</param>
		internal PrivateGroupMessagePacket(UInt32 PrivateGroupID, String FormattedText, VoiceBlob VoiceCommand): 
			base(Packet.Type.PRIVGRP_MSG)
		{
            this.AddData(AoConvert.HostToNetworkOrder(PrivateGroupID));
			this.AddData(new AoString(FormattedText));

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
			this.AddData(popUnsignedInteger(ref data, ref offset));
			this.AddData(popUnsignedInteger(ref data, ref offset));
			this.AddData(popString(ref data, ref offset).ToString());

			String blob = popString(ref data, ref offset).ToString();
			if (blob != String.Empty && blob != "\0" && blob.Trim().Length > 0)
				this.AddData(new VoiceBlob(blob, false));
		}
		internal UInt32 PrivateGroupID
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
        internal string Message
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 3)
				{
					return null;
				}
                Object o = this.Data[2];
                return (String)o;
			}
		}
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
	/// Holds event args for Private Group messages.
	/// </summary>
	public class PrivateChannelMessageEventArgs : EventArgs
	{
		private readonly UInt32 _privGroupID = 0;
		private readonly UInt32 _senderID = 0;
        private readonly string _message = null;
		private readonly VoiceBlob _blob = null;
        private bool _local = false;

		/// <summary>
		/// private group message event message constructor
		/// </summary>
		/// <param name="PrivateGroupID">The id of the private chat group.</param>
		/// <param name="SenderID">The buddy id of the sender.</param>
		/// <param name="Message">Message containing text and click links</param>
		/// <param name="VoiceCommand">voice blob in the message, if any</param>
        public PrivateChannelMessageEventArgs(UInt32 PrivateGroupID, UInt32 SenderID, string Message, VoiceBlob VoiceCommand)
		{
			this._privGroupID = PrivateGroupID;
			this._senderID = SenderID;
			this._message = Message;
			this._blob = VoiceCommand;
		}

		/// <summary>
		/// id of the group where the message originated
		/// </summary>
        public UInt32 PrivateGroupID { get { return this._privGroupID; } }

		/// <summary>
		/// id of the sender
		/// </summary>
        public UInt32 SenderID { get { return this._senderID; } }

		/// <summary>
		/// message contents containing text and click links
		/// </summary>
        public string Message { get { return this._message; } }

		/// <summary>
		/// voice blob of the message, if any
		/// </summary>
        public VoiceBlob VoiceCommand { get { return this._blob; } }

        /// <summary>
        /// set true when message is from the bot's own private channel
        /// </summary>
        public bool Local
        {
            get { return this._local; }
            set { this._local = value; }
        }
	}
}
