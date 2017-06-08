using System;
using System.Net;

namespace AoLib.Net
{
	/// <summary>
	/// Class to handle buddy name lookups
	/// </summary>
	internal class NameLookupPacket : Packet
	{
		/// <summary>
		/// constructor for incomming packets
		/// </summary>
		/// <param name="type">the Packet.Type of the message</param>
		/// <param name="data">the byte array from the socket</param>
		internal NameLookupPacket(Packet.Type type, byte[] data): base(type, data) {}

		/// <summary>
		/// constructor for outgoing packets
		/// </summary>
		/// <param name="name">the name to lookup</param>
		internal NameLookupPacket(String name): 
			base(Packet.Type.NAME_LOOKUP)
		{
			this.AddData(new AoString(name));
		}

		/// <summary>
		/// Maps the byte array into readable data
		/// </summary>
		/// <param name="data">the byte array</param>
		override protected void BytesToData(byte[] data)
		{
			if (data == null || data.Length < 6) { return; }

			int offset = 0;
			this.AddData(popUnsignedInteger(ref data, ref offset));
			this.AddData(popString(ref data, ref offset));
            if ((UInt32)this.Data[0] == UInt32.MaxValue) // It's more reasonable for non-existing characters to have ID 0
                this.Data[0] = UInt32.MinValue;
		}

		/// <summary>
		/// read-only parameter containing the buddy id
		/// </summary>
		internal UInt32 BuddyID
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
		/// read-only paramter containing the buddy name
		/// </summary>
		internal String BuddyName
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 2)
				{
					return null;
				}
                Object o = this.Data[1];
                return o.ToString();
			}
		}
		
	}

	/// <summary>
	/// Holds event args for character name messages.
	/// </summary>
	public class NameLookupEventArgs : EventArgs
	{
		private readonly UInt32 _charID = 0;
		private readonly String _name = null;

		/// <summary>
		/// Constructor for name lookup events
		/// </summary>
		/// <param name="CharacterID">the buddy id</param>
		/// <param name="Name">the name of the buddy</param>
		public NameLookupEventArgs(UInt32 BudddyID, String Name)
		{
			this._charID = BudddyID;
			this._name = Name;
		}

		/// <summary>
		///  read-only property containing the buddy id
		/// </summary>
        public UInt32 BuddyID { get { return this._charID; } }

		/// <summary>
		/// read-only property containing the buddy name
		/// </summary>
        public String Name { get { return this._name; } }
	}
}
