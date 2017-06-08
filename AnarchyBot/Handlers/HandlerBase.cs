namespace AnarchyBot.Handlers
{
    using Discord.WebSocket;
    using System;
    using System.IO;

    // handler base class
    public abstract class HandlerBase
    {
        // verify method confirms that this is the correct handler for the command and executes as required
        public bool Verify(SocketMessage message, string command, string parameter, Action<string> sendResponse, Action<Stream, string, string> sendResponseFile)
        {
            // make sure the command name matches
            if (!command.Equals(this.Command, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            // make sure the parameter has been supplied if we need one
            if (this.NeedsParameter && string.IsNullOrWhiteSpace(parameter))
            {
                return false;
            }

            // if we've made it this far we can process the handler
            this.Process(message, parameter, sendResponse, sendResponseFile);

            // let the manager know that we were successful
            return true;
        }

        // abstract method for the 'do' code
        public abstract void Process(SocketMessage message, string parameter, Action<string> sendResponse, Action<Stream, string, string> sendResponseFile);

        // the command name
        public abstract string Command { get; }

        // whether or not the handler needs a parameter
        public abstract bool NeedsParameter { get; }

        public abstract string HelpText { get; }
    }
}
