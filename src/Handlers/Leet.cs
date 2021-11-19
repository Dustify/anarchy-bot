// with reference to: https://bitbucket.org/Vhab/vhabot/src/86697d44056cfb5a8f50e19ef00c70e66fa87c13/Plugins.Default/vh_Social.cs?at=default&fileviewer=file-view-default
namespace AnarchyBot.Handlers
{
    using Discord.WebSocket;
    using System;
    using System.IO;

    public class Leet : HandlerBase
    {
        public override string Command => "leet";

        public override bool NeedsParameter => false;

        public override string HelpText => "- Leet quotes";

        public string[] Values => new string[] {
               "joo suxxor",
                "ph4t l3wt",
                "*drool*",
                "peekay",
                "r u nubi",
                "i'll get my main!",
                "CREDZplzkthxbye",
                "im uber",
                "foo",
                "NEED MONEY PLZ",
                "CAN I HAVE UR SWORD?",
                "WHY DID U LOOT ME?",
                "whats ur equip?",
                "foo",
                "hoot",
                "coo",
                "stfu mofo"
            };

        public Random R => new Random();

    public override void Process(SocketMessage message, string parameter, Action<string> sendResponse, Action<Stream, string, string> sendResponseFile)
        {
            var result = this.Values[this.R.Next(this.Values.Length)];

            sendResponse(result);
        }
    }
}
