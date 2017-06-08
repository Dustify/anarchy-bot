using System;
using System.Net;
using System.Collections;
using System.Text;

namespace AoLib.Net
{
    /// <summary>
    /// The base packet for all aoChat packets
    /// </summary>
    /// <remarks>
    /// All packets are derived from this base class.
    ///
    ///     Each packet should override the BytesToData function to
    ///     translate incomming socket data to a readable format.
    ///
    ///     A constructor is provided in this base class to initialize
    ///     the read-only type and direction members.
    ///
    /// </remarks>
    public class Packet
    {
        #region Packet Type Enumeration
        public enum Type: short
        {
            // server packet types
            NULL                = -1,       /* I made this one up */
            LOGIN_SEED          = 0,        /* [string Seed] */
            LOGIN_OK            = 5,        /* - */
            LOGIN_ERROR         = 6,        /* [string Message] */
            LOGIN_CHARLIST      = 7,        /* {[int UserID]} {[string Name]} {[int Level]} {[int Online]} */

            CLIENT_UNKNOWN      = 10,       /* [int UserID] */

            CLIENT_NAME         = 20,       /* [int UserID] [string Name] */
            //LOOKUP_RESULT     = 21,       /* [int UserID] [string Name] */

            MSG_PRIVATE         = 30,       /* [int UserID] [string Text] [string Blob] */
            MSG_VICINITY        = 34,       /* [int UserID] [string Text] [string Blob] */
            MSG_ANONVICINITY    = 35,       /* [string] [string Text] [string Blob] */
            MSG_SYSTEM          = 36,       /* [string Text] */
            MESSAGE_SYSTEM      = 37,       /* [int ClientID] [int WindowID] [int MessageID] [String MsgArgs] */

            BUDDY_STATUS        = 40,       /* [int UserID] [int Online] [string Status] */
            BUDDY_REMOVED       = 41,       /* [int UserID] */

            PRIVGRP_INVITE      = 50,       /* [int UserID] */
            PRIVGRP_KICK        = 51,       /* [int UserID] */
            PRIVGRP_JOIN        = 52,       /* [int UserID] */
            PRIVGRP_PART        = 53,       /* [int UserID] */
            PRIVGRP_KICKALL     = 54,       /* - */
            PRIVGRP_CLIJOIN     = 55,       /* [int UserID] [int UserID] */
            PRIVGRP_CLIPART     = 56,       /* [int UserID] [int UserID] */
            PRIVGRP_MSG         = 57,       /* [int UserID] [int UserID] [string Text] [string Blob] */

            GROUP_JOIN          = 60,       /* [grp GroupID] [string GroupName] [word Mute] [int ?] */
            GROUP_PART          = 61,       /* [grp GroupID] */
            //GROUP_MSG         = 65,       /* [grp GroupID] [int UserID] [string Text] [string Blob] */

            //PONG              = 100,      /* [string Whatever] */

            FORWARD             = 110,      /* [int ?] [?] */
            AMD_MUX_INFO        = 1100, /* {[int ?]} {[int ?]} {[int ?]} */

            // In packet types

            LOGIN_RESPONSE      = 2,        /* [int ?] [string Username] [string Key] */
            LOGIN_SELCHAR       = 3,        /* [int UserID] */

            NAME_LOOKUP         = 21,       /* [string Name] */

            //MSG_PRIVATE           = 30,       /* [int UserID] [string Text] [string Blob] */

            BUDDY_ADD           = 40,       /* [int UserID] [string Status] */
            BUDDY_REMOVE        = 41,       /* [int UserID] */
            ONLINE_STATUS       = 42,       /* [int ?] */

            GROUP_DATASET       = 64,       /* [grp GroupID] [word Mute] [int ?] */
            GROUP_MESSAGE       = 65,       /* [grp GroupID] [string text] [string Blob] */
            GROUP_CLIMODE       = 66,       /* [grp GroupID] [int ?] [int ?] [int ?] [int ?] sent when zoning in game? */

            CLIMODE_GET         = 70,       /* [int ?] [grp GroupID] */
            CLIMODE_SET         = 71,       /* [int ?] [int ?] [int ?] [int ?] */

            PING                = 100,      /* [string Whatever] */

            CHAT_COMMAND        = 120       /* [string Command] [string Value]... */
        }
        #endregion // Packet Types

        /// <summary>
        /// the maximum amount of data that can be sent in a single packet.
        /// </summary>
        public const Int32 MaxPacketSize = 8000;

        /// <summary>
        /// the Packet.Type of the message
        /// </summary>
        protected readonly Packet.Type _type;

        /// <summary>
        /// the byte array containing packet data
        /// </summary>
        private readonly byte[] _bytes = null;

        /// <summary>
        /// the array of packet data in readable format
        /// </summary>
        private ArrayList _msg;

        /// <summary>
        /// the encoding used to convert bytes into strings
        /// </summary>
        protected Encoding _enc = Encoding.GetEncoding("utf-8");

        protected PacketQueue.Priority _priority = PacketQueue.Priority.Standard;
        public PacketQueue.Priority Priority
        {
            get { return this._priority; }
            set { this._priority = value; }
        }

        /// <summary>
        /// the default constructor
        /// </summary>
        internal Packet()
        {
            this._msg = new ArrayList();
        }

        /// <summary>
        /// Constructor for setting packet type and direction
        /// </summary>
        /// <param name="type">the Packet.Type of the message</param>
        /// <param name="dir">the direction of the packet, in or out</param>
        internal Packet(Packet.Type type) { this._type = type; }

        /// <summary>
        /// the constructor for incomming packets
        /// </summary>
        /// <param name="type">the Packet.Type of the message</param>
        /// <param name="data">the byte array containing socket data</param>
        internal Packet(Packet.Type type, byte[] data)
        {
            BytesToData(data);
            this._type = type;
        }

        /// <summary>
        /// The read-only type of the message
        /// </summary>
        /// <value>returns the Packet.Type</value>
        internal Packet.Type PacketType
        {
            get { return this._type; }
        }
        /// <summary>
        /// the read-only data in the packet
        /// </summary>
        /// <value>returns a byte array containing packet information</value>
        internal byte[] PacketData
        {
            get { return this._bytes; }
        }

        /// <summary>
        /// the encoding for conversion of bytes to strings
        /// </summary>
        /// <value>returns a conversion encoding method</value>
        protected Encoding Encoding
        {
            get { return this._enc; }
        }

        /// <summary>
        /// Method for adding data to be sent in a specific location in the array.
        /// </summary>
        /// <param name="index">place to insert the data</param>
        /// <param name="o">object to insert into the array</param>
        internal void AddData(int index, object o)
        {
            if (this._msg == null)
            {
                this._msg = new ArrayList();
            }
            this._msg.Insert(index, o);
        }

        /// <summary>
        /// Adds an object to the end of the data array
        /// </summary>
        /// <param name="o">the object to add</param>
        internal void AddData(object o)
        {
            if (this._msg == null)
            {
                this._msg = new ArrayList();
            }
            this._msg.Add(o);
        }

        /// <summary>
        /// The data in readable format
        /// </summary>
        /// <value>returns the arraylist containing packet data
        /// </value>
        internal ArrayList Data
        {
            get { return this._msg; }
        }

        /// <summary>
        /// Method for pulling a string off the byte array.
        /// </summary>
        /// <param name="data">the byte array</param>
        /// <param name="offset">index to start pulling from the array</param>
        /// <returns>returns an AoString object</returns>
        internal static AoString popString(ref byte[] data, ref Int32 offset)
        {
            if (data == null || data.Length - offset < 3)
                return new AoString(String.Empty);

            short len = popShort(ref data, ref offset);
            AoString ret;
            if (data.Length >= len && len > 0)
            {
                ret = new AoString(Encoding.GetEncoding("utf-8").GetString(data, offset, len));
            }
            else
            {
                ret = new AoString("");
            }
            offset += len;
            return ret;
        }

        /// <summary>
        /// Method for pulling a byte array off the byte array.
        /// </summary>
        /// <param name="data">the byte array</param>
        /// <param name="offset">index to start pulling from the array</param>
        /// <returns>returns a byte array</returns>
        internal static byte[] popData(ref byte[] data, ref Int32 offset)
        {
            if (data == null || data.Length - offset < 3)
                return null;

            short len = popShort(ref data, ref offset);
            if (len < 0)
                return new byte[0];
            byte[] ret = new byte[len];
            Array.Copy(data, offset, ret, 0, len);
            offset += len;
            return ret;
        }

        /// <summary>
        /// Method for pulling an integer from the byte array.
        /// </summary>
        /// <param name="data">the byte array</param>
        /// <param name="offset">index where to start pulling from the array</param>
        /// <returns>returns a 4-byte integer</returns>
        internal static Int32 popInteger(ref byte[] data, ref Int32 offset)
        {
            if (data.Length - offset < 4)
                return 0;

            Int32 ret = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, offset));
            offset += 4;
            return ret;
        }

        /// <summary>
        /// Method for pulling an unsigned integer from the byte array.
        /// </summary>
        /// <param name="data">the byte array</param>
        /// <param name="offset">index where to start pulling from the array</param>
        /// <returns>returns a 4-byte unsigned integer</returns>
        internal static UInt32 popUnsignedInteger(ref byte[] data, ref Int32 offset)
        {
            if (data.Length - offset < 4)
                return 0;

            UInt32 ret = AoConvert.NetworkToHostOrder(BitConverter.ToUInt32(data, offset));
            offset += 4;
            return ret;
        }

        /// <summary>
        /// Method for pulling a 5-byte channel id off the array
        /// </summary>
        /// <param name="data">byte array containing the data</param>
        /// <param name="offset">the index where to begin pulling off the array</param>
        /// <returns>5-byte channel id</returns>
        internal static BigInteger popChannelID(ref byte[] data, ref Int32 offset)
        {
            if (data.Length - offset < 5)
                return 0;

            byte[] bi = new byte[5];
            bi[0] = data[offset++];
            bi[1] = data[offset++];
            bi[2] = data[offset++];
            bi[3] = data[offset++];
            bi[4] = data[offset++];
            BigInteger ret = new BigInteger(bi);
            return ret;
        }

        /// <summary>
        /// Method for pulling a short (2-byte) integer off the array.
        /// </summary>
        /// <param name="data">the byte array</param>
        /// <param name="offset">index where to begin pulling off the array</param>
        /// <returns>a short (2-byte) integer</returns>
        internal static Int16 popShort(ref byte[] data, ref Int32 offset)
        {
            if (data.Length - offset < 2)
                return 0;

            Int16 ret = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, offset));
            offset += 2;
            return ret;
        }

        /// <summary>
        /// Method for converting the array data into bytes for transmission over the socket.
        /// </summary>
        /// <returns>a byte array</returns>
        protected virtual byte[] DataToBytes()
        {
            ArrayList a = new ArrayList();
            int len = 0;

            if (this.Data == null)
                return new byte[0];

            foreach (Object o in this.Data)
            {
                byte[] b = null;
                String name = "";
                if (o != null)
                    name = o.GetType().FullName;
                else
                    System.Diagnostics.Trace.WriteLine("Trying to send a null?");

                switch (name)
                {
                    case "":
                        b = new byte[] { 0 };
                        break;
                    case "System.Boolean":
                        b = BitConverter.GetBytes((Boolean)o);
                        break;
                    case "System.Char":
                        b = BitConverter.GetBytes((Char)o);
                        break;
                    case "System.Double":
                        b = BitConverter.GetBytes((Double)o);
                        break;
                    case "System.Int16":
                        b = BitConverter.GetBytes((Int16)o);
                        break;
                    case "System.Int32":
                        b = BitConverter.GetBytes((Int32)o);
                        break;
                    case "System.Int64":
                        b = BitConverter.GetBytes((Int64)o);
                        break;
                    case "System.Single":
                        b = BitConverter.GetBytes((Single)o);
                        break;
                    case "System.UInt16":
                        b = BitConverter.GetBytes((UInt16)o);
                        break;
                    case "System.UInt32":
                        b = BitConverter.GetBytes((UInt32)o);
                        break;
                    case "System.UInt64":
                        b = BitConverter.GetBytes((UInt64)o);
                        break;
                    case "System.Byte[]":
                        b = (byte[])o;
                        break;
                    case "System.Byte":
                        b = new byte[] { (byte)o };
                        break;
                    case "System.String":
                        b = new AoString(o.ToString()).GetBytes();
                        break;
                    default:
                        if (o.GetType() == typeof(BigInteger) || o.GetType().IsSubclassOf(typeof(BigInteger)))
                            b = ((BigInteger)o).getBytes();
                        else if (o.GetType() == typeof(AoLib.AoString) || o.GetType().IsSubclassOf(typeof(AoLib.AoString)))
                            b = ((AoString)o).GetBytes();
                        else if (o.GetType() == typeof(AoLib.VoiceBlob) || o.GetType().IsSubclassOf(typeof(AoLib.VoiceBlob)))
                            b = ((VoiceBlob)o).GetBytes((this.PacketType == Packet.Type.MSG_PRIVATE));
                        else
                            System.Diagnostics.Trace.WriteLine("Hmm... no idea how to process type: " + o.ToString());
                        break;
                }

                if (b != null)
                {
                    len += b.Length;
                    a.Add(b);
                }
            } // end foreach

            if (a.Count == 0)
                return null;
            byte[] ret = new byte[len];
            int i = 0;
            foreach (byte[] d in a)
            {
                d.CopyTo(ret, i);
                i += d.Length;
            }
            return ret;
        }

        /// <summary>
        /// Method for getting the packet data as a byte array
        /// </summary>
        /// <returns>a byte array</returns>
        virtual internal byte[] GetBytes()
        {
            return this.DataToBytes();
        }

        /// <summary>
        /// Abstract method for converting byte array into a readable format.
        /// </summary>
        /// <param name="data">a byte array</param>
        /// <remarks>All derived packets should contain an implementation for this method.</remarks>
        virtual protected void BytesToData(byte[] data) {}

    } // End Packet
}
