namespace AnarchyBot.Handlers
{
    using System;
    using System.IO;

    public class Help : HandlerBase
    {
        public override string Command
        {
            get
            {
                return "help";
            }
        }

        public override bool NeedsParameter
        {
            get
            {
                return false;
            }
        }

        public override void Process(string parameter, Action<string> sendResponse, Action<Stream, string, string> sendResponseFile)
        {
            sendResponse("\n!help - this command\n!whois [name] - get character info\n!level [level] - get level info");
        }
    }
}
