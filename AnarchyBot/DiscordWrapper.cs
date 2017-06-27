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

    public class DiscordWrapper
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

        public Manager Manager { get; private set; }

        public DiscordWrapper(Manager manager)
        {
            this.Manager = manager;

            // add handlers to collection, automated with reflection
            var assembly = System.Reflection.Assembly.GetCallingAssembly();
            var types = assembly.GetTypes();

            foreach (var type in types)
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

            this.Begin();
        }
        
        private async void Begin()
        {
            // we're expecting to see 'token.txt' in the same directory as the executable, this file should only contain the discord app bot token thing
            var token = File.ReadAllText("token.txt").Trim();

            // instantiate discord client object
            var client = new DiscordSocketClient();
            // wire events
            client.Log += this.Log;
            client.MessageReceived += this.MessageReceived;
            client.GuildMemberUpdated += this.GuildMemberUpdated;
            
            // login & 'start'
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
        }

        private Regex streamRegex = new Regex(@"^ao$|ao | ao| anarchy|anarchy |^anarchy$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        // let's check to see if the user just started streaming
        private Task GuildMemberUpdated(SocketGuildUser oldState, SocketGuildUser newState)
        {
            var oldGame = oldState.Game;
            var newGame = newState.Game;

            // if new game doesn't exist or does exist but doesn't have a title or does have a title but doesn't match regex we're not interested
            if (!newGame.HasValue || string.IsNullOrEmpty(newGame.Value.Name) || !streamRegex.IsMatch(newGame.Value.Name))
            {
                return Task.CompletedTask;
            }
            
            // if old game was already streaming we're not interested
            if (oldGame.HasValue && !string.IsNullOrWhiteSpace(oldGame.Value.StreamUrl))
            {
                return Task.CompletedTask;
            }

            // at this point if the new game has a stream url we can assume the user just started streaming
            if (!string.IsNullOrWhiteSpace(newGame.Value.StreamUrl))
            {
                // grab useful stuff
                var streamUrl = newGame.Value.StreamUrl;
                var gameName = newGame.Value.Name;

                // format basic message
                var message = string.Format("{0} just started streaming", newState.Username);

                // if we know the name of the game then add it
                if (!string.IsNullOrEmpty(gameName))
                {
                    message += " " + gameName;
                }

                // punctuation is nice
                message += ".";
                
                // wrap in standard response 
                message = this.WrapResponse(message);

                // add a newline for appearance and add stream URL.
                message += "\n" + streamUrl;

                // send
                this.Manager.UnifiedLog(string.Format("{0}/{1} {2}", newState.Guild.Name, newState.Guild.DefaultChannel.Name, message), LogType.Green);
                newState.Guild.DefaultChannel.SendMessageAsync(message);
            }

            return Task.CompletedTask;
        }

        // discord library's native logging thing
        private Task Log(LogMessage message)
        {
            // just send message to our logging
            this.Manager.UnifiedLog(message.ToString());

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
            this.Manager.UnifiedLog(identifier + request, LogType.Grey);

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
                    this.Manager.UnifiedLog(response, LogType.Green);
                    await message.Channel.SendMessageAsync(response);
                };

            // send response but with a file!
            Action<Stream, string, string> sendResponseFile =
                async (Stream stream, string filename, string response) =>
                {
                    // wrap response
                    response = this.WrapResponse(response);

                    // log and send with file
                    this.Manager.UnifiedLog(response, LogType.Green);
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
