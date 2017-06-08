using System;
using System.Net;

namespace AoLib.Net
{
	/// <summary>
	/// Summary description for TellPacket.
	/// </summary>
	internal class TellPacket : Packet
	{
        private UInt32 _buddyID = 0;
        private AoString _message = null;
		/// <summary>
		/// Constructor for incomming packet data.
		/// </summary>
		/// <param name="type">The Packet.Type of the message</param>
		/// <param name="data">The byte array containing packet data</param>
		internal TellPacket(Packet.Type type, byte[] data): base(type, data) {}

		/// <summary>
		/// Constructor for outgoing packet data
		/// </summary>
		/// <param name="BuddyID">The id of the buddy to who to send the message.</param>
		/// <param name="FormattedText">The contents of the message.  Can contain click links and color formatting in HTML.</param>
		/// <param name="VoiceCommand">The Voice command to send, if any.  Can be null.</param>
		internal TellPacket(UInt32 BuddyID, String FormattedText, VoiceBlob VoiceCommand): 
			base(Packet.Type.MSG_PRIVATE)
		{
            this._buddyID = BuddyID;
            this._message = new AoString(FormattedText);

            this.AddData(AoConvert.HostToNetworkOrder(this._buddyID));
            this.AddData(this._message);

			if (VoiceCommand == null)
				this.AddData(new AoString("\0"));
			else
				this.AddData(VoiceCommand);
		}
		override protected void BytesToData(byte[] data)
		{
			if (data == null || data.Length < 4) { return; }

			int offset = 0;
            this._buddyID = popUnsignedInteger(ref data, ref offset);
            this._message = popString(ref data, ref offset);

			this.AddData(this._buddyID);
			this.AddData(this._message);

			String blob = popString(ref data, ref offset).ToString();
			if (blob != String.Empty && blob != "\0" && blob.Trim().Length > 0)
				this.AddData(new VoiceBlob(blob, false));
		}
        internal UInt32 SenderID
		{
			get 
			{
                if (this._buddyID != 0)
                    return this._buddyID;

				if (this.Data == null || this.Data.Count < 1)
				{
					return 0;
				}
                Object o = this.Data[0];
                return (UInt32)o;
			}
		}
        internal string Message
		{
			get 
			{
                if (this._message != null)
                    return this._message.ToString();

				if (this.Data == null || this.Data.Count < 2)
				{
					return null;
				}
                Object o = this.Data[1];
                return ((AoString)o).ToString();
			}
		}
		internal VoiceBlob VoiceCommand
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 3)
				{
					return null;
				}
                try
                {
                    Object o = this.Data[2];
                    return (VoiceBlob)o;
                }
                catch
                {
                    return null;
                }
			}
		}
	}

	/// <summary>
	/// Holds event args for tell messages.
	/// </summary>
	public class TellEventArgs : EventArgs
	{
        private readonly UInt32 _senderID = 0;
        private readonly string _msg = null;
		private readonly VoiceBlob _blob = null;
        private readonly bool _outgoing = false;

		/// <summary>
		/// Tell message event argument constructor
		/// </summary>
		/// <param name="SenderID">The buddy id of the sender.</param>
		/// <param name="Message">Message containing text and click links</param>
		/// <param name="VoiceCommand">voice blob in the message, if any</param>
        public TellEventArgs(UInt32 SenderID, string Message, VoiceBlob VoiceCommand, bool outgoing)
		{
			this._senderID = SenderID;
			this._msg = Message;
			this._blob = VoiceCommand;
            this._outgoing = outgoing;
		}

		/// <summary>
		/// The buddy id of the sender.
		/// </summary>
        public UInt32 SenderID { get { return this._senderID; } }
		/// <summary>
		/// The originating message containing any text and click links.
		/// </summary>
        public string Message { get { return this._msg; } }
		/// <summary>
		/// The voice command, if any.
		/// </summary>
        public VoiceBlob VoiceCommand { get { return this._blob; } }
        /// <summary>
        /// Whether the message is an outgoing or incomming message
        /// </summary>
        public bool Outgoing { get { return this._outgoing; } }
	}
}
