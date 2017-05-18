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
        // fancy lamda expression to avoid properly defining static Main method, thus saving 5 seconds
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

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

            // help command
            if (command.Equals("help", StringComparison.InvariantCultureIgnoreCase))
            {
                // generate response text
                var response = "```\nBleep bloop. I only do !whois [name]\n```";

                // log & send
                this.UnifiedLog(response, LogType.Green);
                await message.Channel.SendMessageAsync(response);

                return;
            }

            // if no parameter was provided then stop
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return;
            }

            // whois command
            if (command.Equals("whois", StringComparison.InvariantCultureIgnoreCase))
            {
                // store response in single object /////optimisatiooooonnnnnnn//////
                var response = string.Empty;

                // generate character info request url, json please
                var requestUrl = string.Format("http://people.anarchy-online.com/character/bio/d/5/name/{0}/bio.xml?data_type=json", parameter);
                // get data using webclient
                var result = this.webClient.DownloadString(requestUrl);
                
                if (result == "null")
                {
                    // the web api returns the text 'null' if it can't find the character you requested
                    // generate response, log, send & stop
                    response = string.Format("```\nBleep bloop. Couldn't find character '{0}'.\n```", parameter);

                    this.UnifiedLog(response, LogType.Green);
                    await message.Channel.SendMessageAsync(response);

                    return;
                }
                
                // convert data to dynamic (ugh) using JSON.NET
                var allData = (dynamic)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                
                // general character information
                var mainData = allData[0];
                // org information if the character has an org
                var orgData = allData[1];
                // last updated timestamp
                var updated = allData[2];

                // generate 'head' url
                var headUrl = string.Format("http://cdn.funcom.com/billing_files/AO_shop/face/{0}.jpg", mainData.HEADID);
                // download the jpeg
                var headData = this.webClient.DownloadData(headUrl);
                // copy it into a memory stream
                var headStream = new MemoryStream(headData);

                // start generating a response
                response = "```\n";

                // add the general character info
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

                // 'cast' the org data
                IDictionary<string, Newtonsoft.Json.Linq.JToken> orgDataDictionary = orgData;

                // if there is no org then it will be null
                if (orgDataDictionary != null)
                {
                    // add org info to response
                    response +=
                        string.Format(
                            " {0} of {1}.",
                            orgData.RANK_TITLE,
                            orgData.NAME);
                }

                // remove double spaces from response, this might happen if the character doesn't have a first and last name
                response = response.Replace("  ", " ");

                // add last updated and finish response
                response += string.Format(" (Updated {0}).\n```", updated);

                // log & send
                this.UnifiedLog(response, LogType.Green);
                await message.Channel.SendFileAsync(headStream, "head.jpg", response);

                // dispose stream
                headStream.Dispose();
                headStream = null;

                // stop

                return;
            }
        }
    }
}
