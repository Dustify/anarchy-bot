namespace AnarchyBot.Handlers
{
    using Discord.WebSocket;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class Help : HandlerBase
    {
        public override string Command => "help";

        public override bool NeedsParameter => false;

        public override string HelpText => "- This command";

        public List<HandlerBase> Handlers { get; set; }

        public override void Process(SocketMessage message, string parameter, Action<string> sendResponse, Action<Stream, string, string> sendResponseFile)
        {
            var response = "\n";

            foreach(var handler in this.Handlers)
            {
                response += string.Format("!{0} {1}\n", handler.Command, handler.HelpText);
            }

            //sendResponse("\n!help - this command\n!whois [name] - get character info\n!le//vel [level] - get level info\n!oe [skill level] - check skill level over-equipping");

            sendResponse(response);
        }
    }
}
