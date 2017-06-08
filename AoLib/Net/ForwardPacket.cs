using System;
using System.Net;
using System.Collections;

namespace AoLib.Net
{
	/// <summary>
	/// Summary description for the odd forward message
	/// </summary>
	/// <remarks>
	/// This class will probably never get used.  Its probably only used by GMs.
	/// </remarks>
	internal class ForwardPacket : Packet
	{
		internal ForwardPacket(Packet.Type type, byte[] data): base(type, data) {}
		override protected void BytesToData(byte[] data)
		{
			if (data == null || data.Length < 5) { return; }
			int offset = 0;
			this.AddData(popInteger(ref data, ref offset));
			this.AddData(popInteger(ref data, ref offset));		
		}
		// Properties
		internal Int32 ID1
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 1)
				{
					return 0;
				}
                Object o = this.Data[0];
                return (Int32)o;
			}
		}
		internal Int32 ID2
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 2)
				{
					return 0;
				}
                Object o = this.Data[1];
                return (Int32)o;
			}
		}

	}

	/// <summary>
	/// Class for holding event args for AO Chat system message events.
	/// </summary>
	public class ForwardEventArgs : EventArgs
	{
		private readonly Int32 _id1 = 0;
		private readonly Int32 _id2 = 0;

		// Constructor
		public ForwardEventArgs(Int32 ID1, Int32 ID2)
		{
			this._id1 = ID1;
			this._id2 = ID2;
		}

		// Properties
		public Int32	ID1		{	get	{ return this._id1;	}	}
		public Int32	ID2		{	get	{ return this._id2;	}	}
	}
}
