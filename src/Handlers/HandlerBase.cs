namespace AnarchyBot.Handlers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    // handler base class
    public abstract class HandlerBase
    {
        // reused response prefix & suffix
        private const string ResponsePrefix = "```\nBleep bloop. ";
        private const string ResponseSuffix = "\n```";

        // wraps some text in the prefix & suffix
        private string WrapResponse(string response)
        {
            return ResponsePrefix + response + ResponseSuffix;
        }

        // basic send response method, text only
        protected async Task SendResponse(HandlerRequest request, string response)
        {
            // wrap response
            response = this.WrapResponse(response);

            // log & send
            Logger.Log(response, LogType.Green);
            await request.Message.Channel.SendMessageAsync(response);
        }

        // send response but with a file!
        protected async Task SendResponseFile(HandlerRequest request, Stream stream, string filename, string response)
        {
            // wrap response
            response = this.WrapResponse(response);

            // log and send with file
            Logger.Log(response, LogType.Green);
            await request.Message.Channel.SendFileAsync(stream, filename, response);

            // dispose stream
            stream.Dispose();
            stream = null;
        }

        // verify method confirms that this is the correct handler for the command and executes as required
        public async Task<bool> Verify(HandlerRequest request)
        {
            // make sure the command name matches
            if (!request.Command.Equals(this.Command, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            // make sure the parameter has been supplied if we need one
            if (this.NeedsParameter && string.IsNullOrWhiteSpace(request.Parameter))
            {
                return false;
            }

            // if we've made it this far we can process the handler
            await this.Process(request);

            // let the manager know that we were successful
            return true;
        }

        // abstract method for the 'do' code
        protected abstract Task Process(HandlerRequest request);

        // the command name
        public abstract string Command { get; }

        // whether or not the handler needs a parameter
        protected abstract bool NeedsParameter { get; }

        public abstract string HelpText { get; }
    }
}
