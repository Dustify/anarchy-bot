using System;
using System.Net;

namespace AoLib.Net
{
	/// <summary>
	/// Class for handling AO Chat system messages.
	/// </summary>
	internal class SimpleStringPacket : Packet
	{
		internal SimpleStringPacket(): base() {}
		internal SimpleStringPacket(Packet.Type type, byte[] data): base(type, data) {}
		
		override protected void BytesToData(byte[] data)
		{
			if (data == null || data.Length < 3) { return; }

			int offset = 0;
			this.AddData(popString(ref data, ref offset));
		}
		internal String Message
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
		
	} // End aoChatSystemMessage

	/// <summary>
	/// Class for holding event args for AO Chat system message events.
	/// </summary>
	public class SimpleStringPacketEventArgs : EventArgs
	{
		private readonly String _text = null;

		// Constructor
		public SimpleStringPacketEventArgs(String Message)
		{
			this._text = Message;
		}

		// Properties
		public String		Message		{	get	{ return this._text;	}	}
	}
}
