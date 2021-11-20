namespace AnarchyBot
{
    using AnarchyBot.Handlers;
    using Discord;
    using Discord.WebSocket;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class DiscordWrapper
    {
        // store all handlers in a collection
        private List<HandlerBase> Handlers = new List<HandlerBase>();

        public DiscordSocketClient Client { get; private set; }

        public bool IsDisconnected =>
            this.Client == default(DiscordSocketClient) ||
            this.Client.ConnectionState == ConnectionState.Disconnected;

        private async Task ClientDisconnected(Exception arg)
        {
            Logger.Log("Disconnection event", LogType.Yellow);

            await this.Client.StopAsync();
            this.Client.Dispose();
            this.Client = null;
        }

        public async Task Execute()
        {
            // add handlers to collection, automated with reflection
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            var types = assembly.GetTypes();

            var filteredTypes = types.Where(x => x.BaseType != null && x.BaseType.FullName == "AnarchyBot.Handlers.HandlerBase");

            foreach (var type in filteredTypes)
            {
                var constructor = type.GetConstructor(new Type[] { });
                var instance = constructor.Invoke(new object[] { });

                if (instance is Help)
                {
                    ((Help)instance).Handlers = this.Handlers;
                }

                this.Handlers.Add((HandlerBase)instance);
            }

            this.Client = new DiscordSocketClient();

            // wire events
            this.Client.Log += this.Log;
            this.Client.MessageReceived += this.MessageReceived;

            this.Client.Disconnected += this.ClientDisconnected;

            var token = Environment.GetEnvironmentVariable("TOKEN");

            if (File.Exists("token.txt"))
            {
                token = File.ReadAllText("token.txt").Trim();
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new Exception("Token not supplied via 'token.txt' or 'TOKEN' environment variable");
            }

            // login & 'start'
            try
            {
                await this.Client.LoginAsync(TokenType.Bot, token);
                await this.Client.StartAsync();
            }
            catch (Exception exception)
            {
                Logger.Log($"Error starting discord client: {exception.Message}", LogType.Red);
            }
        }

        // discord library's native logging thing
        private Task Log(LogMessage message)
        {
            // just send message to our logging
            Logger.Log(message.ToString());

            // return this because it needs it for whatever reason
            return Task.CompletedTask;
        }

        // generic regex that will allow '!something' or '!something param' and not much else
        private Regex generalRegex = new Regex(@"^!([\w\d]*)\s{0,1}([\w\d]*)$");

        // discord library message received handler
        private async Task MessageReceived(SocketMessage message)
        {
            var server = "Direct";

            if (message.Channel is SocketGuildChannel)
            {
                server = ((SocketGuildChannel)message.Channel).Guild.Name;
            }

            // generate channel and user identification, is server in there somewhere as well?
            var identifier = $"{server}/{message.Channel.Name}/{message.Author.Username}";

            // store the message text
            var content = message.Content;

            // log reception of message
            Logger.Log($"{identifier} {content}", LogType.Grey);

            // try a regex match
            var match = this.generalRegex.Match(content);

            if (!match.Success)
            {
                // if it doesnt match then forget it
                return;
            }

            // extract command & param
            var command = match.Groups[1].Value;
            var parameter = match.Groups[2].Value;

            var request = new HandlerRequest
            {
                Command = command,
                Parameter = parameter,
                Message = message
            };

            // iterate all handlers
            foreach (var handler in this.Handlers)
            {
                // attempt to verify, if valid the handler will do what it needs to do and return true
                var result = await handler.Verify(request);

                // if the handler returned true then we don't need to continue
                if (result)
                {
                    return;
                }
            }
        }
    }
}
