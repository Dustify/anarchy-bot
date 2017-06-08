using System;
using System.Net;
using System.Collections;

namespace AoLib.Net
{
	/// <summary>
	/// Class for the odd Amd Mux Info packet.
	/// </summary>
	internal class AmdMuxInfoPacket : Packet
	{
		/// <summary>
		/// Constructor used for incomming packets
		/// </summary>
		/// <param name="type">the Packet.Type of this packet</param>
		/// <param name="data">the byte array from the socket</param>
		internal AmdMuxInfoPacket(Packet.Type type, byte[] data): base(type, data) {}

		/// <summary>
		/// Maps the byte array into readable data
		/// </summary>
		/// <param name="data">the byte array</param>
		override protected void BytesToData(byte[] data)
		{
			if (data == null || data.Length < 3) { return; }
			
			// an unknown data packet
			this.AddData(new BigInteger(data));
		}

		/// <summary>
		/// An unknown message
		/// </summary>
		internal BigInteger Message
		{
			get 
			{ 
				// make sure we've got something, and if so return it
				if (this.Data == null || this.Data.Count < 1)
				{
					return 0;
				}
                Object o = this.Data[0];
                return (BigInteger)o;
			}
		}

	} // End aoChatAmdMuxInfo

	/// <summary>
	/// Class for holding event args for AO Chat Amd Mux Info message events.
	/// </summary>
	public class AmdMuxInfoEventArgs : EventArgs
	{
		private readonly BigInteger _msg = null;

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="Message">an unknown message</param>
		public AmdMuxInfoEventArgs(BigInteger Message)
		{
			this._msg = Message;
		}

		/// <summary>
		/// An unknown message
		/// </summary>
		public BigInteger	Message		{	get	{ return this._msg;	}	}
	}
}
