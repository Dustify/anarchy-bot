using System;
using System.Text;
using AoLib.Net;

namespace AoLib
{
    /// <summary>
    /// Summary description for VoiceBlob.
    /// </summary>
    public class VoiceBlob
    {
        public enum VoiceStyle
        {
            simple,
            distinguished,
            cool,
            military
        }
        public enum VoiceCommand
        {
            no,
            yes,
            heal,
            help,
            inc,
            run
        }

        public const Int32 voicePrefix = 33751296;

        protected byte[] raw = null;
        protected bool tell = false;
        protected VoiceStyle style = VoiceStyle.simple;
        protected VoiceCommand cmd = VoiceCommand.no;
        protected Encoding _enc = Encoding.GetEncoding("utf-8");

        public VoiceBlob() {}
        internal VoiceBlob(byte[] data, bool FromTell)
        {
            this.tell = FromTell;
            buildBlob(data);
        }
        public VoiceBlob(VoiceStyle Style, VoiceCommand Command)
        {
            this.style = Style;
            this.cmd = Command;
        }
        internal VoiceBlob(String blob, bool FromTell)
        {
            this.tell = FromTell;
            if (string.IsNullOrEmpty(blob) || blob.Trim().Length == 0)
                return;

            buildBlob(this._enc.GetBytes(blob));
        }
        private void buildBlob(byte[] data)
        {
            if (data == null || data.Length < 16)
                return;

            this.raw = data;
            try
            {
                int i = (this.tell ? 1 : 0);

                byte[] pre = new byte[4];
                pre[0] = 0;
                pre[1] = data[i+0];
                pre[2] = data[i+1];
                pre[3] = data[i+2];

                if (BitConverter.ToInt32(pre, 0) == voicePrefix)
                {
                    i += 3;
                    this.style = (VoiceStyle)Enum.Parse(typeof(VoiceStyle), Packet.popString(ref data, ref i).ToString(), true);
                    this.cmd = (VoiceCommand)Enum.Parse(typeof(VoiceCommand), Packet.popString(ref data, ref i).ToString(), true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }
        public override string ToString()
        {
            return this.ToString("Voice Blob");
        }

        public string ToString(String Title)
        {
            if (raw != null && raw.Length > 0)
                System.Diagnostics.Trace.WriteLine(BitConverter.ToString(raw));

            StringBuilder sb = new StringBuilder();

            sb.Append("<a href=");
            sb.Append("\"voiceref://" + VoiceBlob.voicePrefix);
            sb.Append("/" + this.style.ToString());
            sb.Append("/" + this.cmd.ToString());
            sb.Append("/\">");
            sb.Append(Title);
            sb.Append("</a>");

            return sb.ToString();
        }
        public byte[] GetBytes(bool forTell)
        {
            byte[] ret = null;
            long offset = (forTell ? 1 : 0);

            AoString sStyle = new AoString(this.style.ToString());
            AoString sCmd = new AoString(this.cmd.ToString());

            ret = new byte[offset + 3 + sStyle.TotalLength + 2 + sCmd.TotalLength];
            ret.Initialize();

            ret[offset++] = 0x01;
            ret[offset++] = 0x03;
            ret[offset++] = 0x02;

            sStyle.GetBytes().CopyTo(ret, offset);
            sCmd.GetBytes().CopyTo(ret, offset);

            AoString str = new AoString(this._enc.GetString(ret));
            return str.GetBytes();
        }
        public bool isTell
        {
            get { return this.tell; }
            set { this.tell = value; }
        }
    }

//  public class OldBlob
//  {
//      public const Int32 textPrefix = 0x0000C350;
//      public const Int32 voicePrefix = 33751296;
//
//      protected byte[] raw = null;
//      protected bool tell = false;
//      protected bool item = false;
//      protected bool voice = false;
//
//      // for character or text blobs
//      protected Int32 CharacterID = 0;
//      protected AoString Message = null;
//
//      // for item blobs
//      protected Int32 lowID = 0;
//      protected Int32 highID = 0;
//      protected Int16 ql = 0;
//
//      // for voice blobs
//      protected AoString voiceStyle = null;
//
//      public OldBlob() {}
//      internal OldBlob(byte[] data, bool FromTell)
//      {
//          this.tell = FromTell;
//          buildBlob(data);
//      }
//      public OldBlob(Int32 LowID, Int32 HighID, Int16 QL)
//      {
//          this.lowID = LowID;
//          this.highID = HighID;
//          this.ql = QL;
//          this.item = true;
//      }
//      public OldBlob(Int32 CharacterID, String Message)
//      {
//          this.CharacterID = CharacterID;
//          this.Message = new AoString(Message);
//          this.item = false;
//      }
//      public OldBlob(String Message)
//      {
//          Random rand = new Random();
//          this.CharacterID = rand.Next();
//          this.Message = new AoString(Message);
//          this.item = false;
//      }
//      public OldBlob(String voiceStyle, String voiceEffect)
//      {
//          this.voiceStyle = new AoString(voiceStyle);
//          this.Message = new AoString(voiceEffect);
//          this.voice = true;
//      }
//      internal OldBlob(String blob, bool FromTell)
//      {
//          this.tell = FromTell;
//          if (blob == null)
//              return;
//
//          buildBlob(Encoding.GetEncoding("utf-8").GetBytes(blob));
//      }
//      private void buildBlob(byte[] data)
//      {
//          if (data == null || data.Length < 16)
//              return;
//
//          this.raw = data;
//          try
//          {
//              int i = (this.tell ? 1 : 0);
//
//              byte[] pre = new byte[4];
//              pre[0] = 0;
//              pre[1] = data[i+0];
//              pre[2] = data[i+1];
//              pre[3] = data[i+2];
//
//              if (BitConverter.ToInt32(pre, 0) == voicePrefix)
//              {
//                  i += 3;
//                  this.voice = true;
//                  this.voiceStyle = aoPacket.popString(ref data, ref i);
//                  this.Message = aoPacket.popString(ref data, ref i);
//
//                  return;
//              }
//
//              i = (this.tell ? 2 : 1);
//              Int32 low = aoPacket.popInteger(ref data, ref i);
//
//              if (low == textPrefix)
//              {
//
//                  // Text blob
//                  this.CharacterID = aoPacket.popInteger(ref data, ref i);
//                  i += 6;
//                  this.Message = aoPacket.popString(ref data, ref i);
//              }
//              else if (low > 0)
//              {
//                  // Item blob
//                  this.item = true;
//                  this.lowID = low;
//                  this.highID = aoPacket.popInteger(ref data, ref i);
//                  i += 2;
//                  this.ql = aoPacket.popShort(ref data, ref i);
//              }
//          }
//          catch (Exception ex)
//          {
//              Trace.WriteLine(ex.ToString());
//          }
//      }
//      public override string ToString()
//      {
//          String Title = String.Empty;
//          if (this.item)
//              Title = "Item";
//          else if (this.voice)
//              Title = "Voice";
//          else
//              Title = "Blob";
//
//          return this.ToString(Title);
//      }
//
//      public string ToString(String Title)
//      {
//          if (raw != null && raw.Length > 0)
//              Trace.WriteLine(BitConverter.ToString(raw));
//
//          if (! this.item && ! this.voice && (this.Message == null || this.Message.Length == 0))
//              return String.Empty;
//
//          StringBuilder sb = new StringBuilder();
//          sb.Append("<a href=");
//          if (this.item)
//          {
//              sb.Append("\"itemref://" + this.lowID);
//              sb.Append("/" + this.highID);
//              sb.Append("/" + this.ql + "/\">");
//          }
//          else if (this.voice)
//          {
//              sb.Append("\"voiceref://" + aoBlob.voicePrefix);
//              sb.Append("/" + this.voiceStyle);
//              sb.Append("/" + this.Message);
//              sb.Append("/\">");
//          }
//          else
//          {
//              sb.Append("\"charref://" + aoBlob.textPrefix);
//              sb.Append("/" + this.CharacterID);
//              sb.Append("/" + this.Message);
//              sb.Append("/\">");
//          }
//          sb.Append(Title);
//          sb.Append("</a>");
//
//          return sb.ToString();
//      }
//      public byte[] GetBytes(bool forTell)
//      {
//          byte[] ret = null;
//          long offset = (forTell ? 2 : 1);
//
//          if (this.item)
//          {
//              if (forTell)
//                  ret = new byte[16];
//              else
//                  ret = new byte[17];
//
//              ret.Initialize();
//
//              BitConverter.GetBytes(IPAddress.HostToNetworkOrder(this.lowID)).CopyTo(ret, offset);
//              offset += 4;
//              BitConverter.GetBytes(IPAddress.HostToNetworkOrder(this.highID)).CopyTo(ret, offset);
//              offset += 6;
//              BitConverter.GetBytes(IPAddress.HostToNetworkOrder(this.ql)).CopyTo(ret, offset);
//          }
//          else if (this.voice)
//          {
//              offset -= 1;
//
//              ret = new byte[offset + 3 + this.Message.TotalLength + this.voiceStyle.TotalLength];
//              ret.Initialize();
//              ret[offset++] = 0x01;
//              ret[offset++] = 0x03;
//              ret[offset++] = 0x02;
//
//              this.voiceStyle.GetBytes().CopyTo(ret, offset);
//              offset += this.voiceStyle.TotalLength;
//              this.Message.GetBytes().CopyTo(ret, offset);
//          }
//          else
//          {
//              AoString msg = this.Message;
//
//              ret = new byte[msg.TotalLength + offset + 14];
//              BitConverter.GetBytes(IPAddress.HostToNetworkOrder(aoBlob.textPrefix)).CopyTo(ret, offset+=4);
//              BitConverter.GetBytes(IPAddress.HostToNetworkOrder(this.CharacterID)).CopyTo(ret, offset+=10);
//              msg.GetBytes().CopyTo(ret, offset);
//          }
//
//          AoString str = new AoString(Encoding.GetEncoding("utf-8").GetString(ret));
//          return str.GetBytes();
//      }
//      public bool isTell
//      {
//          get { return this.tell; }
//          set { this.tell = value; }
//      }
//  }
}
