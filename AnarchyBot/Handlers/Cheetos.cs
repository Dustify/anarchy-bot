// with reference to: https://bitbucket.org/Vhab/vhabot/src/86697d44056cfb5a8f50e19ef00c70e66fa87c13/Plugins.Default/vh_Social.cs?at=default&fileviewer=file-view-default
namespace AnarchyBot.Handlers
{
    using Discord.WebSocket;
    using System;
    using System.IO;

    public class Cheetos : HandlerBase
    {
        public override string Command => "cheetos";

        public override bool NeedsParameter => false;

        public override string HelpText => "- Ask for a cheeto related incident";

        public string[] Values => new string[] {
                "AnarchyBot casts Materialize Greater Cheetos in {0}'s mouth.",
                "AnarchyBot stuffs some cheetos in your mouth.",
                "Your mom asked me to give you an apple instead.",
                "AnarchyBot cleans her orange hands, then puts more cheetos in {0}'s mouth.",
                "AnarchyBot carefully uses chopsticks to place a cheeto in {0}'s mouth.",
                "AnarchyBot tosses a few cheetos into {0}'s mouth from 11 meters away.",
                "AnarchyBot whips out her QL57 Cheeto Vektor and hits {0} for 82 points of orange damage.",
                "AnarchyBot starts to put a cheeto in {0}'s mouth but pulls away teasingly.",
                "AnarchyBot sneaks a few cheerios in {0}'s mouth instead of cheetos."
            };

        public Random R => new Random();

    public override void Process(SocketMessage message, string parameter, Action<string> sendResponse, Action<Stream, string, string> sendResponseFile)
        {
            var result = this.Values[this.R.Next(this.Values.Length)];

            sendResponse(string.Format(result, message.Author.Username));
        }
    }
}
