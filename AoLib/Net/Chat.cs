using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Timers;
using System.Xml.Serialization;
using AoLib.Net.Login;
using AoLib.Utils;

namespace AoLib.Net
{

    [XmlRoot("dimensions")]
    public class Dimensions
    {
        [XmlElement("dimension")]
        public Dimension[] Dimension;

        public static Dimensions ParseXML( string path )
        {

            if ( ( path == string.Empty ) && File.Exists( "dimensions.xml" ) )
                path = "dimensions.xml";
            else if ( ( path == string.Empty ) &&
                      File.Exists( "data" + Path.DirectorySeparatorChar +
                                   "dimensions.xml" ) )
                path = "data" + Path.DirectorySeparatorChar + "dimensions.xml";

            if ( path == string.Empty )
                return new Dimensions();

            using ( FileStream stream = File.OpenRead( path ) )
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Dimensions));
                Dimensions Servers = (Dimensions)serializer.Deserialize(stream);
                return Servers;
            }
        }

        public Int32 Server( string name )
        {
            for ( int i = 0; i<this.Dimension.Length; i++ )
                if ( this.Dimension[i].Name == name )
                    return this.Dimension[i].ID;
            return -1;
        }

        public string Address( Int32 ID )
        {
            for ( int i = 0; i<Dimension.Length; i++ )
                if ( Dimension[i].ID == ID )
                    return Dimension[i].Address;
            return string.Empty;
        }

        public Int32 Port( Int32 ID )
        {
            for ( int i = 0; i<this.Dimension.Length; i++ )
                if ( this.Dimension[i].ID == ID )
                    return this.Dimension[i].Port;
            return -1;
        }

        public string[] Names()
        {
            string[] ServerNames = new string[this.Dimension.Length];
            for ( Int32 i=0; i<this.Dimension.Length; i++ )
                ServerNames[i] = this.Dimension[i].Name;
            return ServerNames;
        }

    }

    [XmlRoot("dimension")]
    public class Dimension
    {
        [XmlAttribute("id")]
        public string _id;
        [XmlAttribute("name")]
        public string Name;
        [XmlAttribute("serverAddress")]
        public string Address;
        [XmlAttribute("port")]
        public string _port;

        public Int32 ID { get { try { return Convert.ToInt32(this._id); } catch { return 0; } } }
        public Int32 Port { get { try { return Convert.ToInt32(this._port); } catch { return 0; } } }
    }

    public class Server : Dimension
    {

        public Server( string name )
        {
            Dimensions Servers = Dimensions.ParseXML( string.Empty );
            Int32 ID = Servers.Server( name );
            this._id = Convert.ToString( ID );
            this.Name = name;
            this.Address = Servers.Address( ID );
            this._port = Convert.ToString( Servers.Port( ID ) );
        }

        public override string ToString()
        {
            return this.Name;
        }

        public static explicit operator int( Server a )
        {
            return a.ID;
        }

        public static explicit operator String( Server a )
        {
            return a.Name;
        }

        public override bool Equals(System.Object b)
        {
            if ( b == null )
                return false;

            Server a = b as Server;
            if ( (System.Object)a == null )
                return false;

            if ( a.Name == this.Name )
                return true;
            return false;
        }

        public bool Equals( Server b )
        {
            if ( (object)b == null )
                return false;

            if ( this.Name == b.Name )
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }

    	public static bool operator ==( Server a, Server b )
        {
            if ( a.Name == b.Name )
                return true;
            return false;
        }

    	public static bool operator ==( Server a, int b )
        {
            if ( a.ID == b )
                return true;
            return false;
        }

    	public static bool operator ==( int a, Server b )
        {
            if ( a == b.ID )
                return true;
            return false;
        }

    	public static bool operator ==( Server a, string b )
        {
            if ( a.Name == b )
                return true;
            return false;
        }

    	public static bool operator ==( string a, Server b ) 
        {
            if ( a == b.Name )
                return true;
            return false;
        }

    	public static bool operator !=( Server a, Server b ) { return !( a == b ); }
    	public static bool operator !=( Server a, int b ) { return !( a == b ); }
    	public static bool operator !=( int a, Server b ) { return !( a == b ); }
    	public static bool operator !=( Server a, string b ) { return !( a == b ); }
    	public static bool operator !=( string a, Server b ) { return !( a == b ); }

    }

    public enum ChatState
    {
        Disconnected,
        Connecting,
        Login,
        CharacterSelect,
        Connected,
        Reconnecting,
        Error
    }

    public enum ChannelType
    {
        Announcements,
        General,
        Organization,
        German,
        Shopping,
        Towers,
        Unknown
    }

    public class Chat
    {
        public static readonly int BUILD = 20070821;
        public static readonly string VERSION = "0.8.8 LE Beta";

        public bool AutoReconnect = true;
        public int ReconnectDelay = 5000;
        public int PingInterval = 60000;
        public double FastPacketDelay = 10;
        public double SlowPacketDelay = 2200;

        // Events
        public event AmdMuxInfoEventHandler AmdMuxInfoEvent;
        public event AnonVicinityEventHandler AnonVicinityEvent;
        public event BuddyStatusEventHandler BuddyStatusEvent;
        public event CharacterIDEventHandler CharacterIDEvent;
        public event PrivChannelRequestEventHandler PrivateChannelRequestEvent;
        public event NameLookupEventHandler NameLookupEvent;
        public event ForwardEventHandler ForwardEvent;
        public event ChannelJoinEventHandler ChannelJoinEvent;
        public event ChannelMessageEventHandler ChannelMessageEvent;
        public event SystemMessageEventHandler SystemMessageEvent;
        public event SimpleMessageEventHandler SimpleMessageEvent;
        public event LoginOKEventHandler LoginOKEvent;
        public event UnknownPacketEventHandler UnknownPacketEvent;
        public event ClientJoinEventHandler ClientJoinEvent;
        public event PrivChannelMessageEventHandler PrivateGroupMessageEvent;
        public event TellEventHandler TellEvent;
        public event LoginSeedEventHandler LoginSeedEvent;
        public event LoginCharlistEventHandler LoginCharlistEvent;
        public event StatusChangeEventHandler StatusChangeEvent;

        protected Server _dimension;
        protected string _account = string.Empty;
        protected string _password = string.Empty;
        protected string _character = string.Empty;
        protected ChatState _state = ChatState.Disconnected;
        protected UInt32 _id = 0;
        protected BigInteger _organizationid = 0;
        protected string _organization = string.Empty;
        protected bool _ignoreOfflineTells = true;
        protected bool _ignoreAfkTells = true;
        protected bool _ignoreCharacterLoggedIn = true;
        protected bool _removeTempFriends = true;
        protected Thread _receiveThread;
        protected Thread _sendThread;
        protected Socket _socket;
        protected Dictionary<UInt32, String> _users;
        protected Dictionary<BigInteger, String> _channels;
        protected Dictionary<BigInteger, ChannelType> _channelTypes;
        protected string _serverAddress;
        protected int _port;
        protected PacketQueue _fastQueue;
        protected PacketQueue _slowQueue;
        protected bool _closing = false;
        protected System.Timers.Timer _reconnectTimer;
        protected ManualResetEvent _lookupReset;
        protected System.Timers.Timer _pingTimer;
        protected List<UInt32> _offlineTells;
        protected Dictionary<UInt32, DateTime> _friendsOnline;
        protected Dictionary<UInt32, DateTime> _friendsOffline;
        protected List<UInt32> _friendsPending;
        protected Dictionary<UInt32, DateTime> _privateChannel;
        protected DateTime _lastPong = DateTime.Now;

        public UInt32 ID { get { return this._id; } }
        public Server Dimension
        {
            get { return this._dimension; }
            set
            {
                if (this._socket == null || !this._socket.Connected)
                {
                    this._dimension = value;
                    this._serverAddress = value.Address;
                    this._port = value.Port;
                }
            }
        }
        public string Account
        {
            get { return this._account; }
            set { this._account = value; }
        }
        public string Character
        {
            get { return this._character; }
            set { this._character = Format.UppercaseFirst(value); }
        }
        public string Password
        {
            get { return this._password; }
            set { this._password = value; }
        }
        public string Organization { get { return this._organization; } }
        public BigInteger OrganizationID { get { lock (this) { return this._organizationid; } } }
        public ChatState State { get { return this._state; } }
        public bool IgnoreOfflineTells
        {
            get { return this._ignoreOfflineTells; }
            set { this._ignoreOfflineTells = value; }
        }
        public bool IgnoreAfkTells
        {
            get { return this._ignoreAfkTells; }
            set { this._ignoreAfkTells = value; }
        }
        public bool IgnoreCharacterLoggedIn
        {
            get { return this._ignoreCharacterLoggedIn; }
            set { this._ignoreCharacterLoggedIn = value; }
        }
        public bool RemoveTempFriends
        {
            get { return this._removeTempFriends; }
            set { this._removeTempFriends = value; }
        }
        public int SlowQueueCount { get { return this._slowQueue.Count; } }
        public int FastQueueCount { get { return this._fastQueue.Count; } }

        // Constructor
        public Chat() { }

        // Get this thing ready for running
        protected virtual void PrepareChat()
        {
            lock (this)
            {
                if (this._receiveThread != null)
                {
                    if (this._receiveThread.ThreadState == System.Threading.ThreadState.Running)
                    {
                        this._receiveThread.Abort();
                        this._receiveThread.Join(500);
                    }
                }
                if (this._sendThread != null)
                {
                    if (this._sendThread.ThreadState == System.Threading.ThreadState.Running)
                    {
                        this._sendThread.Abort();
                        this._sendThread.Join(500);
                    }
                }
                if (this._socket != null && this._socket.Connected)
                {
                    this._socket.Close();
                }
                this._lookupReset = new ManualResetEvent(false);
                this._receiveThread = new Thread(RunReceiver) {IsBackground = true};
                this._sendThread = new Thread(RunSender) {IsBackground = true};
                this._users = new Dictionary<UInt32, String>();
                this._channels = new Dictionary<BigInteger, String>();
                this._channelTypes = new Dictionary<BigInteger, ChannelType>();
                this._offlineTells = new List<UInt32>();
                this._friendsOnline = new Dictionary<UInt32, DateTime>();
                this._friendsOffline = new Dictionary<UInt32, DateTime>();
                this._friendsPending = new List<UInt32>();
                this._privateChannel = new Dictionary<UInt32, DateTime>();
                this._fastQueue = new PacketQueue {delay = FastPacketDelay};
                this._slowQueue = new PacketQueue {delay = SlowPacketDelay};
                this._reconnectTimer = new System.Timers.Timer {AutoReset = false, Interval = ReconnectDelay};
                this._reconnectTimer.Interval = this.ReconnectDelay;
                this._reconnectTimer.Elapsed += new ElapsedEventHandler(OnReconnectEvent);
                this._pingTimer = new System.Timers.Timer {Interval = PingInterval, AutoReset = true};
                this._pingTimer.Interval = this.PingInterval;
                this._pingTimer.Elapsed += new ElapsedEventHandler(OnPingTimerEvent);
                this._lastPong = DateTime.Now;

                if (string.IsNullOrEmpty(_serverAddress) || _port == 0)
                {
                    Dimensions dims = Dimensions.ParseXML( string.Empty );
                    if( ( dims.Dimension != null ) &&
                        ( dims.Dimension.Length > 0 ) )
                        this.Dimension = new Server(dims.Dimension[0].Name);
                }
            }
        }

        public virtual bool Connect()
        {
            return this.Connect(this.Account, this.Password, this.Character, this.Dimension);
        }
        public virtual bool Connect(string account, string password, string character)
        {
            return this.Connect(account, password, character, this.Dimension);
        }
        public virtual bool Connect(string account, string password, string character, Server dimension)
        {
            lock (this)
            {
                if (this._socket != null && this._socket.Connected)
                {
                    this.Debug("Already Connected", "[Error]");
                    return false;
                }
                this._account = account;
                this._password = password;
                this._character = character;
                this.Dimension = dimension;
                this.OnStatusChangeEvent(new StatusChangeEventArgs(ChatState.Connecting));
                this._closing = false;
                this.PrepareChat();

                this.Debug("Connecting to dimension: " + dimension.ToString(), "[Auth]");
                try
                {
                    IPHostEntry host = Dns.GetHostEntry(this._serverAddress);
                    foreach (IPAddress addy in host.AddressList)
                    {
                        IPEndPoint ipe = new IPEndPoint(addy, this._port);
                        Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        tempSocket.Connect(ipe);

                        if (tempSocket.Connected)
                        {
                            this.Debug("Connected to " + ipe.ToString(), "[Socket]");
                            this._socket = tempSocket;
                            this._receiveThread.Start();
                            this._sendThread.Start();
                            this._pingTimer.Start();
                            return true;
                        }
                        this.Debug("Failed connecting to " + ipe.ToString(), "[Socket]");
                    }
                }
                catch
                {
                    this.Debug("Unknown error during connecting", "[Error]");
                }
            }
            this.OnStatusChangeEvent(new StatusChangeEventArgs(ChatState.Disconnected));
            return false;
        }

        public virtual void Disconnect()
        {
            this._closing = true;
            if (this._reconnectTimer != null) { this._reconnectTimer.Stop(); }
            if (this._pingTimer != null) { this._pingTimer.Stop(); }
            if (this._receiveThread != null)
            {
                if (this._receiveThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    this._receiveThread.Abort();
                    this._receiveThread.Join(new TimeSpan(0,0,5));
                }
                this._receiveThread = null;
            }
            if (this._sendThread != null)
            {
                if (this._sendThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    this._sendThread.Abort();
                    this._sendThread.Join(new TimeSpan(0, 0, 5));
                }
                this._sendThread = null;
            }
            if (this._socket != null && this._socket.Connected) { this._socket.Close(); }
            this._socket = null;
            this._lookupReset = null;
            this._users.Clear();
            this._users = null;
            this._channels.Clear();
            this._channels = null;
            this._offlineTells.Clear();
            this._offlineTells = null;
            this._friendsOnline.Clear();
            this._friendsOnline = null;
            this._friendsOffline.Clear();
            this._friendsOffline = null;
            this._fastQueue = null;
            this._slowQueue = null;
            this._reconnectTimer = null;
            this._pingTimer = null;

            this.OnStatusChangeEvent(new StatusChangeEventArgs(ChatState.Disconnected));
            this._state = ChatState.Disconnected;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        // Receive Thread
        protected void RunReceiver()
        {
            this.Debug("Started", "[ReceiveThread]");
            try
            {
                while (true)
                {
                    if (!_socket.Connected)
                    {
                        throw new Exception("Connection Lost");
                    }
                    byte[] buffer = new byte[4];
                    int receivedBytes = this._socket.Receive(buffer, buffer.Length, 0);
                    if (receivedBytes == 0 || !this._socket.Connected)
                    {
                        throw new Exception("Connection Lost");
                    }
                    Packet.Type type = (Packet.Type)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 0));
                    short lenght = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 2));
                    if (lenght == 0)
                    {
                        ParsePacketData packetData = new ParsePacketData(type, lenght, null);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(this.ParsePacket), packetData);
                    }
                    else
                    {
                        buffer = new byte[lenght];
                        int bytesLeft = lenght;
                        while (bytesLeft > 0)
                        {
                            receivedBytes = this._socket.Receive(buffer, lenght - bytesLeft, bytesLeft, 0);
                            bytesLeft -= receivedBytes;
                            Thread.Sleep(10);
                        }
                        ParsePacketData packetData = new ParsePacketData(type, lenght, buffer);
                        switch (packetData.type)
                        {
                            case Packet.Type.MESSAGE_SYSTEM:
                            case Packet.Type.NAME_LOOKUP:
                            case Packet.Type.PING:
                                this.ParsePacket(packetData);
                                break;
                            default:
                                ThreadPool.QueueUserWorkItem(new WaitCallback(this.ParsePacket), packetData);
                                break;
                        }
                    }
                    Thread.Sleep(10);
                }
            }
            catch { }
            finally
            {
                this.Debug("Stopped!", "[ReceiveThread]");
                this.OnStatusChangeEvent(new StatusChangeEventArgs(ChatState.Disconnected));
            }
        }
        // Send Thread
        protected void RunSender()
        {
            this.Debug("Started", "[SendThread]");
            try
            {
                while (true)
                {
                    if (this._socket == null || this._socket.Connected == false)
                    {
                        throw new Exception("Disconnected");
                    }
                    if (this._fastQueue.Available || this._slowQueue.Available)
                    {
                        Packet packet = _slowQueue.Available ? _slowQueue.Dequeue() : _fastQueue.Dequeue();
                        byte[] data = packet.GetBytes();
                        short len = (short)data.Length;
                        byte[] buffer = new byte[len + 4];
                        BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)packet.PacketType)).CopyTo(buffer, 0);
                        BitConverter.GetBytes(IPAddress.HostToNetworkOrder(len)).CopyTo(buffer, 2);
                        data.CopyTo(buffer, 4);
                        _socket.Send(buffer, buffer.Length, 0);
                        if (packet.PacketType == Packet.Type.MSG_PRIVATE)
                        {
                            try
                            {
                                TellPacket tell = (TellPacket)packet;
                                this.OnTellEvent(new TellEventArgs(tell.SenderID, tell.Message, null, true));
                            }
                            catch { }
                        }
                        Thread.Sleep((int)this.FastPacketDelay);
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch { }
            finally
            {
                this.Debug("Stopped!", "[SendThread]");
                this.OnStatusChangeEvent(new StatusChangeEventArgs(ChatState.Disconnected));
            }
        }

        protected virtual void ParsePacket(Object o)
        {
            ParsePacketData packetData = (ParsePacketData)o;
            Packet packet = null;

            // figure out the packet type and raise an event.
            switch (packetData.type)
            {
                case Packet.Type.PING:
                    OnPongEvent();
                    break;
                case Packet.Type.LOGIN_SEED:
                    packet = new LoginMessagePacket(packetData.type, packetData.data);
                    OnLoginSeedEvent(
                        new LoginMessageEventArgs(
                        ((LoginMessagePacket)packet).Seed
                        ));
                    break;
                case Packet.Type.MSG_SYSTEM:
                case Packet.Type.LOGIN_ERROR:
                    packet = new SimpleStringPacket(packetData.type, packetData.data);
                    OnSimpleMessageEvent(
                        new SimpleStringPacketEventArgs(
                        ((SimpleStringPacket)packet).Message
                        ));
                    break;
                case Packet.Type.LOGIN_CHARLIST:
                    packet = new LoginCharacterListPacket(packetData.type, packetData.data);
                    OnLoginCharlistEvent(
                        new LoginCharlistEventArgs(
                        ((LoginCharacterListPacket)packet).Characters
                        ));
                    break;
                case Packet.Type.BUDDY_REMOVED:
                case Packet.Type.CLIENT_UNKNOWN:
                    packet = new SimpleIdPacket(packetData.type, packetData.data);
                    OnCharacterIDEvent(
                        new CharacterIDEventArgs(
                        ((SimpleIdPacket)packet).CharacterID
                        ));
                    break;
                case Packet.Type.PRIVGRP_INVITE:
                case Packet.Type.PRIVGRP_KICK:
                case Packet.Type.PRIVGRP_PART:
                    System.Diagnostics.Trace.WriteLine(BitConverter.ToString(packetData.data));
                    packet = new PrivateGroupJoinPacket(packetData.type, packetData.data);
                    OnPrivateGroupRequestEvent(
                        new PrivateChannelRequestEventArgs(
                        ((PrivateGroupJoinPacket)packet).SenderID,
                        ((PrivateGroupJoinPacket)packet).Joined
                        ));
                    break;
                case Packet.Type.LOGIN_OK:
                    packet = new EmptyPacket(packetData.type);
                    OnLoginOKEvent();
                    break;
                case Packet.Type.CLIENT_NAME:
                case Packet.Type.NAME_LOOKUP:
                    packet = new NameLookupPacket(packetData.type, packetData.data);
                    OnNameLookupEvent(
                        new NameLookupEventArgs(
                        ((NameLookupPacket)packet).BuddyID,
                        ((NameLookupPacket)packet).BuddyName
                        ));
                    break;
                case Packet.Type.MSG_PRIVATE:
                case Packet.Type.MSG_VICINITY:
                    packet = new TellPacket(packetData.type, packetData.data);
                    OnTellEvent(
                        new TellEventArgs(
                        ((TellPacket)packet).SenderID,
                        ((TellPacket)packet).Message,
                        ((TellPacket)packet).VoiceCommand,
                        false
                        ));
                    break;
                case Packet.Type.MSG_ANONVICINITY:
                    packet = new AnonVicinityPacket(packetData.type, packetData.data);
                    OnAnonVicinityEvent(
                        new AnonVicinityEventArgs(
                        ((AnonVicinityPacket)packet).UnknownString,
                        ((AnonVicinityPacket)packet).Message,
                        ((AnonVicinityPacket)packet).VoiceCommand
                        ));
                    break;
                case Packet.Type.BUDDY_STATUS:
                    packet = new BuddyStatusPacket(packetData.type, packetData.data);
                    OnBuddyStatusEvent(
                        new BuddyStatusEventArgs(
                        ((BuddyStatusPacket)packet).CharacterID,
                        ((BuddyStatusPacket)packet).Status,
                        ((BuddyStatusPacket)packet).BuddyStatus
                        ));
                    break;
                case Packet.Type.GROUP_JOIN:
                    packet = new GroupJoinPacket(packetData.type, packetData.data);
                    OnChannelJoinEvent(
                        new ChannelJoinEventArgs(
                        ((GroupJoinPacket)packet).GroupID,
                        ((GroupJoinPacket)packet).GroupName,
                        ((GroupJoinPacket)packet).Mute,
                        ((GroupJoinPacket)packet).Logging,
                        ((GroupJoinPacket)packet).GroupType
                        ));
                    break;
                case Packet.Type.PRIVGRP_CLIJOIN:
                case Packet.Type.PRIVGRP_CLIPART:
                    packet = new PrivateGroupJoinPacket(packetData.type, packetData.data);
                    OnClientJoinEvent(
                        new ClientJoinEventArgs(
                        ((PrivateGroupJoinPacket)packet).PrivateGroupID,
                        ((PrivateGroupJoinPacket)packet).SenderID,
                        ((PrivateGroupJoinPacket)packet).Joined
                        ));
                    break;
                case Packet.Type.PRIVGRP_MSG:
                    packet = new PrivateGroupMessagePacket(packetData.type, packetData.data);
                    OnPrivateChannelMessageEvent(
                        new PrivateChannelMessageEventArgs(
                        ((PrivateGroupMessagePacket)packet).PrivateGroupID,
                        ((PrivateGroupMessagePacket)packet).SenderID,
                        ((PrivateGroupMessagePacket)packet).Message,
                        ((PrivateGroupMessagePacket)packet).VoiceCommand
                        ));
                    break;
                case Packet.Type.GROUP_MESSAGE:
                    packet = new GroupMessagePacket(packetData.type, packetData.data);
                    OnChannelMessageEvent(
                        new ChannelMessageEventArgs(
                        ((GroupMessagePacket)packet).ChannelID,
                        ((GroupMessagePacket)packet).SenderID,
                        ((GroupMessagePacket)packet).Message,
                        ((GroupMessagePacket)packet).VoiceCommand
                        ));
                    break;
                case Packet.Type.FORWARD:
                    packet = new ForwardPacket(packetData.type, packetData.data);
                    OnForwardEvent(
                        new ForwardEventArgs(
                        ((ForwardPacket)packet).ID1,
                        ((ForwardPacket)packet).ID2
                        ));
                    break;
                case Packet.Type.AMD_MUX_INFO:
                    packet = new AmdMuxInfoPacket(packetData.type, packetData.data);
                    OnAmdMuxInfoEvent(
                        new AmdMuxInfoEventArgs(
                        ((AmdMuxInfoPacket)packet).Message
                        ));
                    break;
                case Packet.Type.MESSAGE_SYSTEM:
                    packet = new SystemMessagePacket(packetData.type, packetData.data);
                    OnSystemMessageEvent(
                        new SystemMessagePacketEventArgs(
                        ((SystemMessagePacket)packet).ClientID,
                        ((SystemMessagePacket)packet).WindowID,
                        ((SystemMessagePacket)packet).MessageID,
                        ((SystemMessagePacket)packet).Message
                        ));
                    break;
                default:
                    if (packetData.type == Packet.Type.NULL && packetData.data != null)
                    {
                        if (BitConverter.ToInt32(packetData.data, 0) == 0 && packetData.data.Length == 4)
                        {
                            Trace.WriteLine("Disconnect packet received.", "[Debug]");
                            this.Disconnect();
                            return;
                        }
                    }
                    packet = new UnknownPacket(packetData.type, packetData.data);
                    OnUnknownPacketEvent(
                        new UnknownPacketEventArgs(
                        ((UnknownPacket)packet).PacketType,
                        ((UnknownPacket)packet).UnknownData
                        ));
                    break;
            } // End switch (packet type)
        }

        #region Events
        protected virtual void OnUnknownPacketEvent(UnknownPacketEventArgs e)
        {
            if (this.UnknownPacketEvent != null)
                this.UnknownPacketEvent(this, e);
        }

        protected virtual void OnSystemMessageEvent(SystemMessagePacketEventArgs e)
        {
            lock (this._offlineTells)
            {
                _offlineTells.Add(e.ClientID);
            }
            if (this.SystemMessageEvent != null)
                this.SystemMessageEvent(this, e);
        }

        protected virtual void OnAmdMuxInfoEvent(AmdMuxInfoEventArgs e)
        {
            if (this.AmdMuxInfoEvent != null)
                this.AmdMuxInfoEvent(this, e);
        }

        protected virtual void OnForwardEvent(ForwardEventArgs e)
        {
            if (this.ForwardEvent != null)
                this.ForwardEvent(this, e);
        }

        protected virtual void OnChannelMessageEvent(ChannelMessageEventArgs e)
        {

            e.Type = this.GetChannelType(e.ChannelID);
            this.Debug(this.GetUserName(e.SenderID) + ": " + e.Message, "[" + this.GetChannelName(e.ChannelID) + "]");

            if (this.ChannelMessageEvent != null)
                this.ChannelMessageEvent(this, e);
        }

        protected virtual void OnPrivateChannelMessageEvent(PrivateChannelMessageEventArgs e)
        {
            if (e.PrivateGroupID == this._id) { e.Local = true; }
            this.Debug(this.GetUserName(e.SenderID) + ": " + e.Message, "[" + this.GetUserName(e.PrivateGroupID) + "]");

            if (this.PrivateGroupMessageEvent != null)
                this.PrivateGroupMessageEvent(this, e);
        }

        protected virtual void OnTellEvent(TellEventArgs e)
        {
            if (this.IgnoreAfkTells)
            {
                string afk = this.GetUserName(e.SenderID) + " is AFK";
                if (e.Message.Length > afk.Length)
                {
                    if (e.Message.Substring(0, afk.Length) == afk)
                    {
                        return;
                    }
                }
            }
            if (this.IgnoreOfflineTells)
            {
                lock (this._offlineTells)
                {
                    if (this._offlineTells.Contains(e.SenderID))
                    {
                        this.Debug(e.Message, "[Offline][" + this.GetUserName(e.SenderID) + "]:");
                        this._offlineTells.Remove(e.SenderID);
                        return;
                    }
                }
            }
            if (e.Outgoing)
                this.Debug(e.Message, "To [" + this.GetUserName(e.SenderID) + "]:");
            else
                this.Debug(e.Message, "[" + this.GetUserName(e.SenderID) + "]:");

            if (this.TellEvent != null)
                this.TellEvent(this, e);
        }

        protected virtual void OnClientJoinEvent(ClientJoinEventArgs e)
        {
            lock (this._privateChannel)
            {
                if (e.Join)
                    this._privateChannel[e.SenderID] = DateTime.Now;
                else
                    if (this._privateChannel.ContainsKey(e.SenderID))
                        this._privateChannel.Remove(e.SenderID);
            }

            if (this.ClientJoinEvent != null)
                this.ClientJoinEvent(this, e);
        }

        protected virtual void OnChannelJoinEvent(ChannelJoinEventArgs e)
        {
            if (this.State != ChatState.Connected)
                this.OnStatusChangeEvent(new StatusChangeEventArgs(ChatState.Connected));

            lock (_channels)
            {
                this._channels[e.GroupID] = e.GroupName;
            }
            switch (e.GroupTypeShort)
            {
                case -32634:
                    e.GroupType = ChannelType.Announcements;
                    break;
                case -32668:
                    e.GroupType = ChannelType.General;
                    break;
                case -32764:
                    e.GroupType = ChannelType.Organization;
                    break;
                case -32700:
                    e.GroupType = ChannelType.German;
                    break;
                case -32412:
                    e.GroupType = ChannelType.Shopping;
                    break;
                case -32762:
                    e.GroupType = ChannelType.Towers;
                    break;
                default:
                    e.GroupType = ChannelType.Unknown;
                    this.Debug("Unknown channel type: " + e.GroupTypeShort, "[Error]");
                    break;
            }
            this.Debug("Joined channel: " + e.GroupName + " (ID:" + e.GroupID + " Type:" + e.GroupType.ToString() + " Muted:" + e.Mute.ToString() + ")", "[Bot]");
            if (e.GroupType == ChannelType.Organization)
            {
                this._organization = e.GroupName;
                this._organizationid = e.GroupID;
                this.Debug("Registered Organization: " + e.GroupName + " (ID:" + e.GroupID + ")", "[Bot]");
            }
            lock (this._channelTypes)
            {
                this._channelTypes[e.GroupID] = e.GroupType;
            }

            if (this.ChannelJoinEvent != null)
                this.ChannelJoinEvent(this, e);
        }

        protected virtual void OnBuddyStatusEvent(BuddyStatusEventArgs e)
        {
            this.Debug("Friend Status Received: " + this.GetUserName(e.CharacterID) + " (ID:" + e.CharacterID + " Online:" + e.Online.ToString() + " Status:" + e.BuddyStatus + ")", "[Database]");

            lock (this._friendsPending)
                if (this._friendsPending.Contains(e.CharacterID))
                    this._friendsPending.Remove(e.CharacterID);

            if (this._removeTempFriends)
            {
                if (e.BuddyStatus == 2)
                {
                    this.SendFriendRemove(e.CharacterID);
                    return;
                }
            }
            e.First = true;
            if (e.Online)
            {
                lock (this._friendsOnline)
                {
                    this._friendsOnline[e.CharacterID] = DateTime.Now;
                }
                lock (this._friendsOffline)
                {
                    if (this._friendsOffline.ContainsKey(e.CharacterID))
                    {
                        this._friendsOffline.Remove(e.CharacterID);
                        e.First = false;
                    }
                }
            }
            else
            {
                lock (this._friendsOffline)
                {
                    this._friendsOffline[e.CharacterID] = DateTime.Now;
                }
                lock (this._friendsOnline)
                {
                    if (this._friendsOnline.ContainsKey(e.CharacterID))
                    {
                        this._friendsOnline.Remove(e.CharacterID);
                        e.First = false;
                    }
                }
            }

            if (this.BuddyStatusEvent != null)
                this.BuddyStatusEvent(this, e);
        }

        protected virtual void OnAnonVicinityEvent(AnonVicinityEventArgs e)
        {
            if (this.AnonVicinityEvent != null)
                this.AnonVicinityEvent(this, e);
        }

        protected virtual void OnNameLookupEvent(NameLookupEventArgs e)
        {
            lock (this._users)
            {
                this._users[e.BuddyID] = Format.UppercaseFirst(e.Name);
            }
            if (e.BuddyID > 0)
            {
                this.Debug("Name lookup received: " + e.Name + " (ID:" + e.BuddyID + ")", "[Database]");
            }
            else
            {
                this.Debug("User doesn't exist: " + e.Name, "[Database]");
            }
            this._lookupReset.Set();
            this._lookupReset.Reset();

            if (this.NameLookupEvent != null)
                this.NameLookupEvent(this, e);
        }

        protected virtual void OnLoginOKEvent()
        {
            this.Debug("Logged in succesfully", "[Auth]");
            if (this.LoginOKEvent != null)
                this.LoginOKEvent(this, new EventArgs());
        }

        protected virtual void OnPrivateGroupRequestEvent(PrivateChannelRequestEventArgs e)
        {
            if (this.PrivateChannelRequestEvent != null)
                this.PrivateChannelRequestEvent(this, e);
        }

        protected virtual void OnCharacterIDEvent(CharacterIDEventArgs e)
        {
            if (this.CharacterIDEvent != null)
                this.CharacterIDEvent(this, e);
        }

        protected virtual void OnLoginCharlistEvent(LoginCharlistEventArgs e)
        {
            if (this.LoginCharlistEvent != null)
                this.LoginCharlistEvent(this, e);

            if (e.Override)
                this._character = Format.UppercaseFirst(e.Character);

            if (!e.CharacterList.Contains(this._character))
            {
                String clist = String.Empty;
                foreach (String chars in e.CharacterList.Keys)
                {
                    clist += chars + " ";
                }
                this.Debug(String.Format("The character name, {0}, was not found in {1}.", this._character, clist.Trim()), "[Auth]");
                this.OnStatusChangeEvent(new StatusChangeEventArgs(ChatState.Error));
            }
            else
            {
                LoginChar character = (LoginChar)e.CharacterList[this._character];
                if (character.IsOnline && !this.IgnoreCharacterLoggedIn)
                {
                    this.Debug("Character " + this._character + " is already online!", "[Auth]");
                    this.OnStatusChangeEvent(new StatusChangeEventArgs(ChatState.Disconnected));
                    return;
                }
                lock (this._users)
                    this._users.Add(character.ID, Format.UppercaseFirst(character.Name));
                this._id = character.ID;
                SimpleIdPacket packet = new SimpleIdPacket(Packet.Type.LOGIN_SELCHAR, character.ID)
                    {
                        Priority = PacketQueue.Priority.Urgent
                    };
                this.SendPacket(packet);
                this.Debug("Selecting character: " + this._character, "[Auth]");
                this.OnStatusChangeEvent(new StatusChangeEventArgs(ChatState.CharacterSelect));
            }
        }

        protected virtual void OnSimpleMessageEvent(SimpleStringPacketEventArgs e)
        {
            this.Debug(e.Message, "[System]");

            if (this.SimpleMessageEvent != null)
                this.SimpleMessageEvent(this, e);
        }

        protected virtual void OnLoginSeedEvent(LoginMessageEventArgs e)
        {
            this.Debug("Logging in with account: " + this._account, "[Auth]");
            this.SendPacket(new LoginMessagePacket(this._account, this._password, e.Seed));
            this.OnStatusChangeEvent(new StatusChangeEventArgs(ChatState.Login));
            if (this.LoginSeedEvent != null)
                this.LoginSeedEvent(this, e);
        }

        protected virtual void OnStatusChangeEvent(StatusChangeEventArgs e)
        {
            if (this._state == ChatState.Reconnecting && e.State == ChatState.Disconnected)
            {
                return;
            }
            if (this._state != e.State)
            {
                if (e.State == ChatState.Disconnected)
                {
                    if (this._pingTimer != null)
                    {
                        this._pingTimer.Stop();
                    }
                }
                if (e.State == ChatState.Disconnected && this._closing == false && this.AutoReconnect == true)
                {
                    this._state = ChatState.Reconnecting;
                    e = new StatusChangeEventArgs(this._state);
                    if (this._socket != null)
                    {
                        if (this._socket.Connected) { this._socket.Close(); }
                    }
                    this._reconnectTimer.Interval = this.ReconnectDelay;
                    this._reconnectTimer.Start();
                }
                this._state = e.State;
                this.Debug("State changed to: " + e.State.ToString(), "[Bot]");
                if (this.StatusChangeEvent != null)
                    this.StatusChangeEvent(this, e);
            }
        }

        protected virtual void OnReconnectEvent(object sender, ElapsedEventArgs e)
        {
            this._reconnectTimer.Stop();
            this.Connect();
        }

        protected virtual void OnPongEvent()
        {
            this.Debug("Pong!", "[Bot]");
            this._lastPong = DateTime.Now;
        }

        protected virtual void OnPingTimerEvent(object sender, ElapsedEventArgs e)
        {
            if (this._socket == null || this._socket.Connected == false)
            {
                this.OnStatusChangeEvent(new StatusChangeEventArgs(ChatState.Disconnected));
            }
            TimeSpan ts = DateTime.Now.Subtract(this._lastPong);
            if (ts.TotalMilliseconds > (this.PingInterval * 1.5))
            {
                this.Debug("Connection timed out", "[Bot]");
                this.OnStatusChangeEvent(new StatusChangeEventArgs(ChatState.Disconnected));
                return;
            }
            this.Debug("Ping?", "[Bot]");
            this.SendPing();
        }
        #endregion

        #region Get Commands
        public virtual UInt32 GetUserID(string user)
        {
            bool Lookup = false;
            user = Format.UppercaseFirst(user);
            for (int i = 0; i < 300; i++)
            {
                lock (this._users)
                {
                    if (this._users.ContainsValue(user))
                    {
                        foreach (KeyValuePair<UInt32, String> kvp in this._users)
                        {
                            if (kvp.Value == user)
                            {
                                if (kvp.Key > 0)
                                {
                                    return kvp.Key;
                                }
                                this._users.Remove(kvp.Key);
                                return 0;
                            }
                        }
                    }
                    else if (Lookup == false)
                    {
                        this.SendNameLookup(user);
                        Lookup = true;
                    }
                }
                Thread.Sleep(50);
            }
            return 0;
        }

        public virtual string GetUserName(UInt32 userID)
        {
            if (userID == 0 || userID == UInt32.MaxValue)
                return "";
            if (this._users == null)
                return "";
            lock (this._users)
            {
                if (this._users.ContainsKey(userID))
                {
                    return this._users[userID];
                }
                return "";
            }
        }

        public virtual BigInteger GetChannelID(String channelName)
        {
            BigInteger ChannelID = new BigInteger(0);
            lock (this._channels)
            {
                if (this._channels.ContainsValue(channelName))
                {
                    foreach (KeyValuePair<BigInteger, String> kvp in this._channels)
                    {
                        if (kvp.Value == channelName)
                            return kvp.Key;
                    }
                }
            }
            return ChannelID;
        }

        public virtual string GetChannelName(Int64 channelID) { return this.GetChannelName(new BigInteger(channelID)); }
        public virtual string GetChannelName(BigInteger channelID)
        {
            if (this._channels == null)
                return "";

            lock (this._channels)
            {
                if (this._channels.ContainsKey(channelID))
                {
                    return this._channels[channelID];
                }
                return "";
            }
        }

        public virtual ChannelType GetChannelType(Int64 channelID) { return this.GetChannelType(new BigInteger(channelID)); }
        public virtual ChannelType GetChannelType(BigInteger channelID)
        {
            lock (this._channelTypes)
            {
                if (this._channelTypes.ContainsKey(channelID))
                {
                    return this._channelTypes[channelID];
                }
                return ChannelType.Unknown;
            }
        }

        public virtual Dictionary<BigInteger, String> GetChannels()
        {
            lock (_channels)
            {
                return _channels;
            }
        }

        public virtual bool GetFriendStatus(string user) { return this.GetFriendStatus(this.GetUserID(user)); }
        public virtual bool GetFriendStatus(UInt32 userID)
        {
            lock (this._friendsOffline)
                if (this._friendsOffline.ContainsKey(userID))
                    return false;

            lock (this._friendsOnline)
                if (this._friendsOnline.ContainsKey(userID))
                    return true;

            this.SendFriendAdd(userID);
            bool online = false;
            for (int i = 0; i < 300; i++)
            {
                lock (this._friendsOffline)
                {
                    if (this._friendsOffline.ContainsKey(userID))
                    {
                        online = false;
                        break;
                    }
                }
                lock (this._friendsOnline)
                {
                    if (this._friendsOnline.ContainsKey(userID))
                    {
                        online = true;
                        break;
                    }
                }
                Thread.Sleep(50);
            }
            this.SendFriendRemove(userID);
            return online;
        }

        public Dictionary<UInt32, DateTime> GetOnlineFriends()
        {
            lock (this._friendsOnline)
            {
                Dictionary<UInt32, DateTime> list = new Dictionary<UInt32, DateTime>(this._friendsOnline);
                return list;
            }
        }

        public Dictionary<UInt32, DateTime> GetOfflineFriends()
        {
            lock (this._friendsOffline)
            {
                Dictionary<UInt32, DateTime> list = new Dictionary<UInt32, DateTime>(this._friendsOffline);
                return list;
            }
        }

        public Int32 GetTotalFriends()
        {
            Int32 total = 0;
            lock (this._friendsOffline)
                total += this._friendsOffline.Count;
            lock (this._friendsOnline)
                total += this._friendsOnline.Count;
            lock (this._friendsPending)
                total += this._friendsPending.Count;

            return total;
        }

        public Dictionary<UInt32, DateTime> GetPrivateChannelMembers()
        {
            lock (this._privateChannel)
            {
                Dictionary<UInt32, DateTime> list = new Dictionary<UInt32, DateTime>(this._privateChannel);
                return list;
            }
        }
        #endregion

        #region Send Commands
        public virtual void SendPacket(Packet packet)
        {
            if (this._socket == null || !this._socket.Connected)
            {
                this.Debug("Not Connected", "[Error]");
                return;
            }
            switch (packet.PacketType)
            {
                case Packet.Type.MSG_PRIVATE:
                case Packet.Type.GROUP_MESSAGE:
                    _slowQueue.Enqueue(packet.Priority, packet);
                    break;
                default:
                    _fastQueue.Enqueue(packet.Priority, packet);
                    break;
            }
        }

        public virtual void SendChannelUpdate(BigInteger channelID, bool mute)
        {
            GroupJoinPacket p = new GroupJoinPacket(channelID, mute);
            this.SendPacket(p);
        }

        public virtual void SendChannelMessage(string channel, string text) { this.SendChannelMessage(this.GetChannelID(channel), text, PacketQueue.Priority.Standard, null); }
        public virtual void SendChannelMessage(BigInteger channelID, string text) { this.SendChannelMessage(channelID, text, PacketQueue.Priority.Standard, null); }
        public virtual void SendChannelMessage(BigInteger channelID, string text, PacketQueue.Priority priority) { this.SendChannelMessage(channelID, text, priority, null); }
        public virtual void SendChannelMessage(BigInteger channelID, string text, PacketQueue.Priority priority, VoiceBlob voice)
        {
            GroupMessagePacket p = new GroupMessagePacket(channelID, text, voice) {Priority = priority};
            this.SendPacket(p);
        }

        public virtual void SendFriendAdd(string user) { this.SendFriendAdd(user, false); }
        public virtual void SendFriendAdd(string user, bool temp) { this.SendFriendAdd(this.GetUserID(user), temp); }
        public virtual void SendFriendAdd(UInt32 userID) { this.SendFriendAdd(userID, false); }
        public virtual void SendFriendAdd(UInt32 userID, bool temp)
        {
            if (userID == 0)
                return;
            if (userID == this._id)
                return;

            this.Debug("Adding user to friendslist: " + this.GetUserName(userID) + " (ID:" + userID + ")", "[Bot]");
            BuddyStatusPacket p = new BuddyStatusPacket(userID, true, temp) {Priority = PacketQueue.Priority.Standard};

            lock (this._friendsPending)
                this._friendsPending.Add(userID);

            this.SendPacket(p);
        }

        public virtual void SendFriendRemove(string user) { this.SendFriendRemove(this.GetUserID(user)); }
        public virtual void SendFriendRemove(UInt32 userID)
        {
            if (userID == this._id)
                return;
            this.Debug("Removing user from friendslist: " + this.GetUserName(userID) + " (ID:" + userID + ")", "[Bot]");
            BuddyStatusPacket p = new BuddyStatusPacket(userID, false, false);
            p.Priority = PacketQueue.Priority.Standard;
            this.SendPacket(p);
            lock (this._friendsOnline)
                if (this._friendsOnline.ContainsKey(userID))
                    this._friendsOnline.Remove(userID);

            lock (this._friendsOffline)
                if (this._friendsOffline.ContainsKey(userID))
                    this._friendsOffline.Remove(userID);

            lock (this._friendsPending)
                if (this._friendsPending.Contains(userID))
                    this._friendsPending.Remove(userID);
        }

        public virtual void SendPrivateChannelInvite(string user) { this.SendPrivateChannelInvite(this.GetUserID(user)); }
        public virtual void SendPrivateChannelInvite(UInt32 userID)
        {
            if (userID == this._id)
                return;
            SimpleIdPacket p = new SimpleIdPacket(Packet.Type.PRIVGRP_INVITE, userID) { Priority = PacketQueue.Priority.Urgent };
            this.SendPacket(p);
        }

        public virtual void SendPrivateChannelKick(string user) { this.SendPrivateChannelKick(this.GetUserID(user)); }
        public virtual void SendPrivateChannelKick(UInt32 userID)
        {
            if (userID == this._id)
                return;
            SimpleIdPacket p = new SimpleIdPacket(Packet.Type.PRIVGRP_KICK, userID);
            p.Priority = PacketQueue.Priority.Urgent;
            this.SendPacket(p);
        }

        public virtual void SendPrivateChannelKickAll()
        {
            EmptyPacket p = new EmptyPacket(Packet.Type.PRIVGRP_KICKALL) {Priority = PacketQueue.Priority.Urgent};
            this.SendPacket(p);
        }

        public virtual void SendPrivateChannelMessage(string text) { this.SendPrivateChannelMessage(this._id, text, null); }
        public virtual void SendPrivateChannelMessage(string channel, string text) { this.SendPrivateChannelMessage(this.GetUserID(channel), text, null); }
        public virtual void SendPrivateChannelMessage(UInt32 channelID, string text) { this.SendPrivateChannelMessage(channelID, text, null); }
        public virtual void SendPrivateChannelMessage(UInt32 channelID, string text, VoiceBlob voice)
        {
            PrivateGroupMessagePacket p = new PrivateGroupMessagePacket(channelID, text, voice) { Priority = PacketQueue.Priority.Urgent };
            this.SendPacket(p);
        }

        public virtual void SendPrivateMessage(string user, string text) { this.SendPrivateMessage(this.GetUserID(user), text, PacketQueue.Priority.Standard, null); }
        public virtual void SendPrivateMessage(UInt32 userID, string text) { this.SendPrivateMessage(userID, text, PacketQueue.Priority.Standard, null); }
        public virtual void SendPrivateMessage(UInt32 userID, string text, PacketQueue.Priority priority) { this.SendPrivateMessage(userID, text, priority, null); }
        public virtual void SendPrivateMessage(UInt32 userID, string text, PacketQueue.Priority priority, VoiceBlob voice)
        {
            if (userID == this._id)
                return;
            TellPacket p = new TellPacket(userID, text, voice) {Priority = priority};
            this.SendPacket(p);
        }

        public virtual void SendNameLookup(string name)
        {
            lock (this._users)
                if (this._users.ContainsValue(Format.UppercaseFirst(name)))
                    return;

            NameLookupPacket p = new NameLookupPacket(name) {Priority = PacketQueue.Priority.Urgent};
            this.Debug("Requesting ID: " + name, "[Database]");
            this.SendPacket(p);
        }

        public virtual void SendPing()
        {
            EmptyPacket p = new EmptyPacket(Packet.Type.PING) {Priority = PacketQueue.Priority.Urgent};
            this.SendPacket(p);
        }
        #endregion

        public override string ToString()
        {
            return this.Character + "@" + this.Dimension.ToString();
        }

        protected void Debug(string msg, string cat)
        {
            Trace.WriteLine("[" + this.Character + "@" + this.Dimension.ToString() + "] " + cat + " " + msg);
        }
    } // end of Chat

    public class StatusChangeEventArgs : EventArgs
    {
        private ChatState _state;
        public StatusChangeEventArgs(ChatState state)
        {
            this._state = state;
        }
        public ChatState State
        {
            get { return this._state; }
        }
    }

    public class ParsePacketData
    {
        public Packet.Type type;
        public short lenght = 0;
        public byte[] data;

        public ParsePacketData(Packet.Type t, short l, byte[] d)
        {
            this.type = t;
            this.lenght = l;
            if (d != null)
            {
                this.data = new byte[d.Length];
                d.CopyTo(this.data, 0);
            }
        }
    }
}
