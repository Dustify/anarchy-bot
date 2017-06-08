using System;
using System.Net;

namespace AoLib.Net
{
	/// <summary>
	/// Class for muting or listening to channels.
	/// </summary>
	internal class GroupJoinPacket : Packet
	{
		/// <summary>
		/// Constructor for incomming packets
		/// </summary>
		/// <param name="type">the Packet.Type of the packet</param>
		/// <param name="data">the byte array from the socket</param>
		internal GroupJoinPacket(Packet.Type type, byte[] data): base(type, data) {}

		/// <summary>
		/// Constructor for outgoing packets
		/// </summary>
		/// <param name="GroupID">the 5-byte id of the channel</param>
		/// <param name="Mute">whether the channel should be muted or not</param>
		internal GroupJoinPacket(BigInteger GroupID, bool Mute): 
			base(Packet.Type.GROUP_DATASET)
		{
			this.AddData(GroupID);
			
			// The following is a hack to get
			// the ability to mute channels.
			// The first two bytes need to be 1 or 0 
			// depending on whether you are muting or not, respectively.
			// The last 5 bytes need to be there, but
			// I've no idea what they are for.
			if (! Mute)
			{
				this.AddData((byte)1);
				this.AddData((byte)1);
			}
			else 
			{
				this.AddData((byte)0);
				this.AddData((byte)0);
			}

			this.AddData((byte)0);
			this.AddData((byte)0);
			this.AddData((byte)0);
			this.AddData((byte)1);
			this.AddData((byte)0);
		}

		/// <summary>
		/// Maps the byte array into readable data
		/// </summary>
		/// <param name="data">the byte array</param>
		override protected void BytesToData(byte[] data)
		{
			if (data == null || data.Length < 10) { return; }

			int offset = 0;
			this.AddData(popChannelID(ref data, ref offset));
			this.AddData(popString(ref data, ref offset));
			this.AddData(popShort(ref data, ref offset));
            this.AddData(popShort(ref data, ref offset));
		}

		/// <summary>
		/// 5-byte channel id
		/// </summary>
		internal BigInteger GroupID
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 1)
				{
					return 0;
				}
                Object o = this.Data[0];
                return (BigInteger)o;
			}
		}

		/// <summary>
		/// name of the channel
		/// </summary>
		internal String GroupName
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 2 || this.Data[1] == null)
				{
					return null;
				}
                Object o = this.Data[1];
                return o.ToString();
			}
		}

		/// <summary>
		/// whether the channel is muted or not
		/// </summary>
		internal bool Mute
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 3 || this.Data[2] == null)
				{
					return false;
				}
                Object o = this.Data[2];
                return Convert.ToBoolean((Int16)o & 0x0100);
			}
		}

		/// <summary>
		/// whether the channel is being logged or not
		/// </summary>
		internal bool Logging
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 3 || this.Data[2] == null)
				{
					return false;
				}
                Object o = this.Data[2];
                return Convert.ToBoolean((Int16)o & 0x0200);
			}
		}

		/// <summary>
		/// Group Type
		/// </summary>
        internal short GroupType
		{
			get 
			{ 
				if (this.Data == null || this.Data.Count < 4)
				{
					return 0;
				}
                Object o = this.Data[3];
                return (short)o;
			}
		}		
	}

	/// <summary>
	/// Holds event args for Group Join messages.
	/// </summary>
	public class ChannelJoinEventArgs : EventArgs
	{
		private readonly BigInteger _groupID = 0;
		private readonly String _groupName = null;
		private readonly bool _mute = false;
		private readonly bool _logging = false;
        private readonly short _groupTypeShort = 0;
        private ChannelType _groupType = ChannelType.Unknown;

		/// <summary>
		/// constructor for channel mute events
		/// </summary>
		/// <param name="GroupID">5-byte channel id</param>
		/// <param name="GroupName">channel name</param>
		/// <param name="Mute">whether the channel is muted or not</param>
		/// <param name="Logging">whether the channel is logging or not</param>
		/// <param name="GroupType">Group Type</param>
        public ChannelJoinEventArgs(BigInteger GroupID, String GroupName, bool Mute, bool Logging, short GroupType)
		{
			this._groupID = GroupID;
			this._groupName = GroupName;
			this._mute = Mute;
			this._logging = Logging;
            this._groupTypeShort = GroupType;
		}

		/// <summary>
		/// The 5-byte channel id
		/// </summary>
        public BigInteger GroupID { get { return this._groupID; } }

		/// <summary>
		/// the channel name
		/// </summary>
        public String GroupName { get { return this._groupName; } }

		/// <summary>
		/// whether the channel is muted or not
		/// </summary>
        public bool Mute { get { return this._mute; } }

		/// <summary>
		/// whether the channel is logging or not
		/// </summary>
        public bool Logging { get { return this._logging; } }

		/// <summary>
		/// GroupTypeShort
		/// </summary>
        public short GroupTypeShort { get { return this._groupTypeShort; } }

        /// <summary>
        /// GroupType
        /// </summary>
        public ChannelType GroupType 
        {
            get 
            {
                return this._groupType;
            }
            set
            {
                this._groupType = value;
            }
        }
	}
}
