namespace AnarchyBot
{
    using AnarchyBot.Handlers;
    using Discord;
    using Discord.WebSocket;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class Manager
    {
        // reused response prefix & suffix
        private const string ResponsePrefix = "```\nBleep bloop. ";
        private const string ResponseSuffix = "\n```";

        // wraps some text in the prefix & suffix
        private string WrapResponse(string response)
        {
            return ResponsePrefix + response + ResponseSuffix;
        }

        // store all handlers in a collection
        private List<HandlerBase> Handlers = new List<HandlerBase>();

        public Manager()
        {
            // add handlers to collection, automated with reflection
            var assembly = System.Reflection.Assembly.GetCallingAssembly();
            var types = assembly.GetTypes();

            foreach(var type in types)
            {
                if (type.BaseType.FullName == "AnarchyBot.Handlers.HandlerBase")
                {
                    var constructor = type.GetConstructor(new Type[] { });
                    var instance = constructor.Invoke(new object[] { });

                    if (instance is Help)
                    {
                        ((Help)instance).Handlers = this.Handlers;
                    }

                    this.Handlers.Add((HandlerBase)instance);
                }
            }

            // let's get this show on the road
            this.MainAsync().GetAwaiter().GetResult();
        }
        
        // main wrapper
        public async Task MainAsync()
        {
            // we're expecting to see 'token.txt' in the same directory as the executable, this file should only contain the discord app bot token thing
            var token = File.ReadAllText("token.txt").Trim();

            // instantiate discord client object
            var client = new DiscordSocketClient();
            // wire events
            client.Log += Log;
            client.MessageReceived += MessageReceived;

            // login & 'start'
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            // apparently this is how we keep things running indefinitely now
            await Task.Delay(-1);
        }

        // quick set of options for console colour when logging
        private enum LogType
        {
            Green,
            Yellow,
            Red,
            Grey
        }

        // locking object, because im oldschool / lazy like that
        private static object LogLock = new object();

        // 'colourless' log method
        private void UnifiedLog(string message)
        {
            this.UnifiedLog(message, LogType.Grey);
        }

        // full log method
        private void UnifiedLog(string message, LogType logType)
        {
            // switch the type and set the colour
            switch (logType)
            {
                case LogType.Green:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogType.Yellow:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogType.Red:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogType.Grey:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }

            // store now, as this is an extra hungry hungry hippo call
            var now = DateTime.Now;
            // format the message with a timestamp, also replace newlines so it doesn't mess our pretty logfiles up
            var fullMessage = string.Format("[{0}] {1}", now, message).Replace("\n", "\\n");

            // LLLLLOOOOCCCKKKK
            lock (LogLock)
            {
                // write to console
                Console.WriteLine(fullMessage);
                // write to log file, generate filename so it's 'once per day'
                File.AppendAllText(now.ToString("yyyy-MM-dd") + "-log.txt", fullMessage + "\r\n");

                // reset colour
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        // discord library's native logging thing
        private Task Log(LogMessage message)
        {
            // just send message to our logging
            this.UnifiedLog(message.ToString());

            // return this because it needs it for whatever reason
            return Task.CompletedTask;
        }

        // generic regex that will allow '!something' or '!something param' and not much else
        private Regex generalRegex = new Regex(@"^!([\w\d]*)\s{0,1}([\w\d]*)$");
        // web client for retrieving character data / images
        private WebClient webClient = new WebClient();

        // discord library message received handler
        private async Task MessageReceived(SocketMessage message)
        {
            // generate channel and user identification, is server in there somewhere as well?
            var identifier =
                string.Format(
                    "{0}/{1} ",
                    message.Channel.Name,
                    message.Author.Username);

            // store the message text
            var request = message.Content;

            // log reception of message
            this.UnifiedLog(identifier + request, LogType.Grey);

            // try a regex match
            var match = this.generalRegex.Match(request);

            if (!match.Success)
            {
                // if it doesnt match then forget it
                return;
            }

            // extract command & param
            var command = match.Groups[1].Value;
            var parameter = match.Groups[2].Value;

            // basic send response method, text only
            Action<string> sendResponse = 
                async (string response) => 
                {
                    // wrap response
                    response = this.WrapResponse(response);

                    // log & send
                    this.UnifiedLog(response, LogType.Green);
                    await message.Channel.SendMessageAsync(response);
                };

            // send response but with a file!
            Action<Stream, string, string> sendResponseFile =
                async (Stream stream, string filename, string response) =>
                {
                    // wrap response
                    response = this.WrapResponse(response);

                    // log and send with file
                    this.UnifiedLog(response, LogType.Green);
                    await message.Channel.SendFileAsync(stream, filename, response);

                    // dispose stream
                    stream.Dispose();
                    stream = null;
                };

            // iterate all handlers
            foreach (var handler in this.Handlers)
            {
                // attempt to verify, if valid the handler will do what it needs to do and return true
                var result = handler.Verify(message, command, parameter, sendResponse, sendResponseFile);

                // if the handler returned true then we don't need to continue
                if (result)
                {
                    return;
                }
            }
        }
    }
}
