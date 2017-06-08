using System;
using System.Net;
using System.Collections;
using AoLib.Utils;

namespace AoLib.Net.Login
{
	/// <summary>
	/// Gets the list of characters under the authenticated account.
	/// </summary>
	internal class LoginCharacterListPacket : Packet
	{
		protected short _numChars;
		protected Hashtable _chars;

		internal LoginCharacterListPacket(Packet.Type type, byte[] data): base(type, data) {}

		override protected void BytesToData(byte[] data)
		{
			if (data == null || data.Length < 3) { return; }

			short num = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 0));
			LoginChar[] cs = new LoginChar[num];
			
			int i = 0;
			int offset = 2;

			for (; i < num; i++)
			{
				//cs[i].ID = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, offset));
                cs[i].ID = AoConvert.ToUInt32(IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, offset)));
				offset += 4;
			}
			num = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, offset));
			offset += 2;
			for (i = 0; i < num; i++)
			{
				AoString s = new AoString(data, offset);
				cs[i].Name = s.ToString();
				offset += s.TotalLength;
			}
			num = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, offset));
			offset += 2;
			for (i = 0; i < num; i++)
			{
				cs[i].Level = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, offset));
				offset += 4;
			}
			num = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, offset));
			offset += 2;
			for (i = 0; i < num; i++)
			{
				cs[i].IsOnline = Convert.ToBoolean(IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, offset)));
				offset += 4;
			}

			this._chars = new Hashtable();
			this._numChars = num;

			foreach (LoginChar c in cs)
			{
				this._chars.Add(c.Name, c);
				this.AddData(c);
			}
		}
		internal Hashtable Characters
		{
			get { return this._chars; }
		}
	}

	/// <summary>
	/// Holds event args for login character list messages.
	/// </summary>
	public class LoginCharlistEventArgs : EventArgs
	{
		private readonly Hashtable _chars = null;

		// Constructor
		public LoginCharlistEventArgs(Hashtable CharacterList)
		{
			this._chars = CharacterList;
		}

		// Properties
		public Hashtable	CharacterList	{	get	{ return this._chars;	}	}
        public bool Override = false;
        public string Character = String.Empty;
	}
}
