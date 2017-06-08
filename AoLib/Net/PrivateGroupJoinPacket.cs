using System;
using System.Net;

namespace AoLib.Net
{
	/// <summary>
	/// Class for private group joins and parts
	/// </summary>
	internal class PrivateGroupJoinPacket : Packet
	{
		/// <summary>
		/// Constructor for incomming packets
		/// </summary>
		/// <param name="type">the Packet.Type of the message</param>
		/// <param name="data">the byte array from the socket</param>
		internal PrivateGroupJoinPacket(Packet.Type type, byte[] data): base(type, data) {}

		/// <summary>
		/// Constructor for outgoing packets
		/// </summary>
		/// <param name="PrivateGroupID"></param>
		/// <param name="SenderID"></param>
		/// <param name="Join"></param>
		internal PrivateGroupJoinPacket(UInt32 PrivateGroupID, UInt32 BuddyID, bool Join): 
			base((Join ? Packet.Type.PRIVGRP_CLIJOIN : Packet.Type.PRIVGRP_CLIJOIN))
		{
            this.AddData(AoConvert.HostToNetworkOrder(PrivateGroupID));
            this.AddData(AoConvert.HostToNetworkOrder(BuddyID));
		}

		/// <summary>
		/// Constructor for outgoing packets
		/// </summary>
		/// <param name="PrivateGroupID">id of the private group</param>
		/// <param name="Join">whether to join or part</param>
		internal PrivateGroupJoinPacket(UInt32 PrivateGroupID, bool Join): 
			base((Join ? Packet.Type.PRIVGRP_JOIN : Packet.Type.PRIVGRP_PART))
		{
            this.AddData(AoConvert.HostToNetworkOrder(PrivateGroupID));
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
		internal bool Joined
		{
			get	{ return (this.PacketType == Packet.Type.PRIVGRP_CLIJOIN); }
		}
	}

	/// <summary>
	/// Class for holding event args for AO Chat system message events.
	/// </summary>
	public class ClientJoinEventArgs : EventArgs
	{
        private readonly UInt32 _privGroupID = 0;
        private readonly UInt32 _senderID = 0;
		private readonly bool _join = false;

		// Constructor
        public ClientJoinEventArgs(UInt32 PrivateGroupID, UInt32 SenderID, bool Join)
		{
			this._privGroupID = PrivateGroupID;
			this._senderID = SenderID;
			this._join = Join;
		}

		// Properties
        public UInt32 PrivateGroupID { get { return this._privGroupID; } }
        public UInt32 SenderID { get { return this._senderID; } }
        public bool Join { get { return this._join; } }
	}
}
