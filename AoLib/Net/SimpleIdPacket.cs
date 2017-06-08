using System;
using System.Net;

namespace AoLib.Net
{
	/// <summary>
	/// Class for a single id
	/// </summary>
	internal class SimpleIdPacket : Packet
	{
		/// <summary>
		/// Constructor for incomming packets
		/// </summary>
		/// <param name="type">the Packet.Type of the packet</param>
		/// <param name="data">the byte array from the socket</param>
		internal SimpleIdPacket(Packet.Type type, byte[] data): base(type, data) {}

		/// <summary>
		/// Constructor for outgoing packets
		/// </summary>
		/// <param name="type">the Packet.Type of the packet</param>
		/// <param name="id">the id to send</param>
		internal SimpleIdPacket(Packet.Type type, UInt32 id): base(type)
		{
            this.AddData(AoConvert.HostToNetworkOrder(id));
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
		}

		/// <summary>
		/// the buddy id
		/// </summary>
		internal UInt32 CharacterID
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

		/// <summary>
		/// if a private group invite packet, will be true
		/// </summary>
		internal bool Joined
		{
			get	{ return (this.PacketType == Packet.Type.PRIVGRP_INVITE); }
		}
	}

	/// <summary>
	/// Class for holding event args for simple buddy id messages
	/// </summary>
	public class CharacterIDEventArgs : EventArgs
	{
		private readonly UInt32 _charID = 0;

		/// <summary>
		/// Event class constructor
		/// </summary>
		/// <param name="CharacterID">the buddy id</param>
		public CharacterIDEventArgs(UInt32 CharacterID)
		{
			this._charID = CharacterID;
		}

		/// <summary>
		/// the buddy id
		/// </summary>
		public UInt32	CharacterID		{	get	{ return this._charID;	}	}
	}

	/// <summary>
	/// Holds event args for private group requests
	/// </summary>
	public class PrivateChannelRequestEventArgs : EventArgs
	{
		private readonly UInt32 _charID = 0;
		private readonly bool _join = false;

		/// <summary>
		/// Constructor for private group requests
		/// </summary>
		/// <param name="CharacterID">the buddy id</param>
		/// <param name="Join">whether asked to join or leave</param>
		public PrivateChannelRequestEventArgs(UInt32 CharacterID, bool Join)
		{
			this._charID = CharacterID;
			this._join = Join;
		}

		/// <summary>
		/// the buddy id
		/// </summary>
        public UInt32 CharacterID { get { return this._charID; } }

		/// <summary>
		/// whether joining or leaving
		/// </summary>
        public bool Join { get { return this._join; } }
	}
}
