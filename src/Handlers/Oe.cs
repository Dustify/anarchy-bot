namespace AnarchyBot.Handlers
{
    using Discord.WebSocket;
    using System;
    using System.IO;

    public class Oe : HandlerBase
    {
        public override string Command => "oe";

        public override string HelpText => "[skill level] - Check skill over-equipping";

        public override bool NeedsParameter => true;

        public override void Process(SocketMessage message, string parameter, Action<string> sendResponse, Action<Stream, string, string> sendResponseFile)
        {
            var level = default(int);
            var parseSuccess = int.TryParse(parameter, out level);

            if (!parseSuccess)
            {
                return;
            }

            var low = Math.Round(level * 0.8, 0);
            var high = Math.Round((level / 8.0) * 10.0);

            var result = string.Format("{0} > {1} > {2}", low, level, high);

            sendResponse(result);
        }
    }
}
