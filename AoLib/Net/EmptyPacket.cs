using System;
using System.Net;

namespace AoLib.Net
{
	/// <summary>
	/// Class for handling no content messages, like OK events
	/// </summary>
	internal class EmptyPacket : Packet
	{
		/// <summary>
		/// constructor for incomming messages
		/// </summary>
		/// <param name="type">the Packet.Type of the message</param>
		internal EmptyPacket(Packet.Type type): base(type) {}

		internal override byte[] GetBytes()
		{
			if (this.PacketType == Packet.Type.PRIVGRP_KICKALL)
				return new byte[0];
            return (new AoString(string.Empty)).GetBytes();
		}

	}
}
