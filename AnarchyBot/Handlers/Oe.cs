namespace AnarchyBot.Handlers
{
    using System;
    using System.IO;

    public class Oe : HandlerBase
    {
        public override string Command => "oe";

        public override bool NeedsParameter => true;

        public override void Process(string parameter, Action<string> sendResponse, Action<Stream, string, string> sendResponseFile)
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
