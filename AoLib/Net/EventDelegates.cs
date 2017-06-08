using System;
using AoLib.Net.Login;

namespace AoLib.Net
{
	//Event Delegates
	public delegate void AmdMuxInfoEventHandler(object sender, AmdMuxInfoEventArgs e);
	public delegate void AnonVicinityEventHandler(object sender, AnonVicinityEventArgs e);
	public delegate void BuddyStatusEventHandler(object sender,	BuddyStatusEventArgs e);
	public delegate void CharacterIDEventHandler(object sender,	CharacterIDEventArgs e);
	public delegate void PrivChannelRequestEventHandler(object sender, PrivateChannelRequestEventArgs e);
	public delegate void NameLookupEventHandler(object sender, NameLookupEventArgs e);
	public delegate void ForwardEventHandler(object sender,	ForwardEventArgs e);
	public delegate void ChannelJoinEventHandler(object sender, ChannelJoinEventArgs e);
	public delegate void ChannelMessageEventHandler(object sender, ChannelMessageEventArgs e);
	public delegate void SimpleMessageEventHandler(object sender, SimpleStringPacketEventArgs e);
	public delegate void SystemMessageEventHandler(object sender, SystemMessagePacketEventArgs e);
	public delegate void LoginOKEventHandler(object sender, EventArgs e);
	public delegate void UnknownPacketEventHandler(object sender, UnknownPacketEventArgs e);
	public delegate void ClientJoinEventHandler(object sender, ClientJoinEventArgs e);
	public delegate void PrivChannelMessageEventHandler(object sender, PrivateChannelMessageEventArgs e);
	public delegate void TellEventHandler(object sender, TellEventArgs e);
	public delegate void LoginSeedEventHandler(object sender, LoginMessageEventArgs e);
	public delegate void LoginCharlistEventHandler(object sender, LoginCharlistEventArgs e);
    public delegate void StatusChangeEventHandler(object sender, StatusChangeEventArgs e);
}