using System;
using System.Net;

namespace AoLib.Net
{
    /// <summary>
    /// Class for handling buddy list additions and removals
    /// </summary>
    internal class BuddyStatusPacket : Packet
    {
        /// <summary>
        /// Constructor used for incomming packets
        /// </summary>
        /// <param name="type">the Packet.Type of the packet</param>
        /// <param name="data">the byte array from the socket</param>
        internal BuddyStatusPacket(Packet.Type type, byte[] data) : base(type, data) { }

        /// <summary>
        /// Constructor used for outgoing packets
        /// </summary>
        /// <param name="id">The character id of the buddy</param>
        /// <param name="AddBuddy">boolean, whether to add or remove buddy</param>
        /// <param name="TempBuddy">boolean, whether to add as temp buddy</param>
        internal BuddyStatusPacket(UInt32 id, bool AddBuddy, bool TempBuddy)
            :
            base((AddBuddy ? Packet.Type.BUDDY_ADD : Packet.Type.BUDDY_REMOVE))
        {
            this.AddData(AoConvert.HostToNetworkOrder(id));

            // if the buddy is to be added, you need to send a string containing "1"
            // if the buddy is to be removed, you need to send an empty packet
            if (AddBuddy && TempBuddy)
            {
                this.AddData(new AoString("0"));
            }
            else if (AddBuddy)
            {
                this.AddData(new AoString("1"));
            }
        }

        /// <summary>
        /// Maps the byte array into readable data
        /// </summary>
        /// <param name="data">the byte array</param>
        override protected void BytesToData(byte[] data)
        {
            if (data == null || data.Length < 10) { return; }

            int offset = 0;
            this.AddData(popUnsignedInteger(ref data, ref offset));
            this.AddData(popInteger(ref data, ref offset));
            this.AddData(popData(ref data, ref offset));
        }

        /// <summary>
        /// the id of the buddy
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
        /// Whether the buddy is added or removed
        /// </summary>
        internal bool Status
        {
            get
            {
                if (this.Data == null || this.Data.Count < 2)
                {
                    return false;
                }
                Object o = this.Data[1];
                return Convert.ToBoolean(o);
            }
        }

        /// <summary>
        /// some other value
        /// </summary>
        internal Int32 BuddyStatus
        {
            get
            {
                if (this.Data == null || this.Data.Count < 3)
                {
                    return 0;
                }
                byte[] buddyData = (byte[])this.Data[2];
                return ((buddyData[0] & 0x01) != 0 ? 1 : 2);
            }
        }

    }

    /// <summary>
    /// Holds event args for Buddy Status messages.
    /// </summary>
    public class BuddyStatusEventArgs : EventArgs
    {
        private readonly UInt32 _charID = 0;
        private readonly bool _status = false;
        private readonly Int32 _buddyStatus = 0;
        private bool _first = false;

        /// <summary>
        /// Event argument constructor
        /// </summary>
        /// <param name="CharacterID">The character id of the buddy</param>
        /// <param name="Status">whether the buddy is added or not</param>
        /// <param name="StatusName">some unknown value</param>
        public BuddyStatusEventArgs(UInt32 CharacterID, bool Online, Int32 BuddyStatus)
        {
            this._charID = CharacterID;
            this._status = Online;
            this._buddyStatus = BuddyStatus;
        }

        /// <summary>
        /// the id of the buddy
        /// </summary>
        public UInt32 CharacterID { get { return this._charID; } }
        /// <summary>
        /// whether the buddy is added or not
        /// </summary>
        public bool Online { get { return this._status; } }
        /// <summary>
        /// buddy type (eg. temp buddy)
        /// </summary>
        public Int32 BuddyStatus { get { return this._buddyStatus; } }
        /// <summary>
        /// Defines if it's the first time this buddy was seen
        /// </summary>
        public bool First { get { return this._first; } set { _first = value; } }
    }
}
