namespace AnarchyBot.Handlers
{
    using System;
    using System.IO;

    public abstract class HandlerBase
    {
        public bool Verify(string command, string parameter, Action<string> sendResponse, Action<Stream, string, string> sendResponseFile)
        {
            if (!command.Equals(this.Command, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (this.NeedsParameter && string.IsNullOrWhiteSpace(parameter))
            {
                return false;
            }

            this.Process(parameter, sendResponse, sendResponseFile);

            return true;
        }

        public abstract void Process(string parameter, Action<string> sendResponse, Action<Stream, string, string> sendResponseFile);

        public abstract string Command { get; }
        public abstract bool NeedsParameter { get; }
    }
}
