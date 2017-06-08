using System;
using System.Net;

namespace AoLib.Net
{
	/// <summary>
	/// Class used when unknown packets are received.
	/// </summary>
	internal class UnknownPacket : Packet
	{
		/// <summary>
		/// Constructor for incomming packets
		/// </summary>
		/// <param name="type">the Packet.Type of the message</param>
		/// <param name="data">the byte array with socket data</param>
		internal UnknownPacket(Packet.Type type, byte[] data): base(type, data) {}

		/// <summary>
		/// Maps the byte array into readable data
		/// </summary>
		/// <param name="data">the byte array</param>
		override protected void BytesToData(byte[] data)
		{
			this.AddData(this.Encoding.GetString(data));
		}

		/// <summary>
		/// The message received, whatever that was
		/// </summary>
		internal String UnknownData
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 1)
				{
					return null;
				}
                return BitConverter.ToString(this.DataToBytes());
			}
		}
	}

	/// <summary>
	/// Class for holding event args for unknown events.
	/// </summary>
	public class UnknownPacketEventArgs : EventArgs
	{
		/// <summary>
		/// Private member to store the message.
		/// </summary>
		private readonly String _text = null;
		private readonly Packet.Type _type = 0;

		/// <summary>
		/// Constructor for creating event args
		/// </summary>
		/// <param name="Message"></param>
		public UnknownPacketEventArgs(Packet.Type type, String Message)
		{
			this._type = type;
			this._text = Message;
		}

		/// <summary>
		/// Message from unknown event
		/// </summary>
		/// <value>returns the message</value>
		public String		Message		{	get	{ return this._text;	}	}
		public Packet.Type	PacketType	{	get { return this._type;	}	}
	}
}
