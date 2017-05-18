namespace AnarchyBot
{
    using Discord;
    using Discord.WebSocket;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var token = File.ReadAllText("token.txt").Trim();

            var client = new DiscordSocketClient();
            client.Log += Log;
            client.MessageReceived += MessageReceived;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private enum LogType
        {
            Green,
            Yellow,
            Red,
            Grey
        }

        private static object LogLock = new object();

        private void UnifiedLog(string message)
        {
            this.UnifiedLog(message, LogType.Grey);
        }

        private void UnifiedLog(string message, LogType logType)
        {
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

            var now = DateTime.Now;
            var fullMessage = string.Format("[{0}] {1}", now, message).Replace("\n", "");

            lock (LogLock)
            {
                Console.WriteLine(fullMessage);
                File.AppendAllText(now.ToString("yyyy-MM-dd") + "-log.txt", fullMessage + "\r\n");

                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private Task Log(LogMessage msg)
        {
            this.UnifiedLog(msg.ToString());

            return Task.CompletedTask;
        }

        private static Regex GeneralRegex = new Regex(@"^!([\w\d]*)\s{0,1}([\w\d]*)$");
        private static WebClient Client = new WebClient();

        private async Task MessageReceived(SocketMessage message)
        {
            var identifier =
                string.Format(
                    "{0}/{1} ",
                    message.Channel.Name,
                    message.Author.Username);

            var request = message.Content;

            this.UnifiedLog(identifier + request, LogType.Grey);

            var match = GeneralRegex.Match(request);

            if (!match.Success)
            {
                return;
            }

            var command = match.Groups[1].Value;
            var parameter = match.Groups[2].Value;

            if (command.Equals("help", StringComparison.InvariantCultureIgnoreCase))
            {
                var response = "```\nBleep bloop. I only do !whois [name]\n```";

                this.UnifiedLog(response, LogType.Green);
                await message.Channel.SendMessageAsync(response);

                return;
            }

            if (command.Equals("whois", StringComparison.InvariantCultureIgnoreCase))
            {
                var response = string.Empty;

                var requestUrl = string.Format("http://people.anarchy-online.com/character/bio/d/5/name/{0}/bio.xml?data_type=json", parameter);
                var result = Client.DownloadString(requestUrl);
                
                if (result == "null")
                {
                    response = string.Format("```\nBleep bloop. Couldn't find character '{0}'.\n```", parameter);

                    this.UnifiedLog(response, LogType.Green);
                    await message.Channel.SendMessageAsync(response);

                    return;
                }
                
                var allData = (dynamic)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                var mainData = allData[0];
                var orgData = allData[1];
                var updated = allData[2];

                var headUrl = string.Format("http://cdn.funcom.com/billing_files/AO_shop/face/{0}.jpg", mainData.HEADID);
                var headData = Client.DownloadData(headUrl);
                var stream = new MemoryStream(headData);

                response = "```\n";

                response +=
                    string.Format(
                        "Bleep bloop. {0} '{1}' {2} is a{3} {4} {5} {6} {7}, level {8}.",
                        mainData.FIRSTNAME,
                        mainData.NAME,
                        mainData.LASTNAME,
                        mainData.SIDE == "Omni" ? "n" : "",
                        mainData.SIDE,
                        mainData.SEX,
                        mainData.BREED,
                        mainData.PROF,
                        mainData.LEVELX);

                IDictionary<string, Newtonsoft.Json.Linq.JToken> orgDataDictionary = orgData;

                if (orgDataDictionary != null)
                {
                    response +=
                        string.Format(
                            " {0} of {1}.",
                            orgData.RANK_TITLE,
                            orgData.NAME);
                }

                response = response.Replace("  ", " ");

                response += string.Format(" (Updated {0}).\n```", updated);

                this.UnifiedLog(response, LogType.Green);
                await message.Channel.SendFileAsync(stream, "head.jpg", response);

                return;
            }
        }
    }
}
