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

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());

            return Task.CompletedTask;
        }

        private static Regex RegexWhois = new Regex(@"^!whois (.*)");
        private static WebClient Client = new WebClient();

        private async Task MessageReceived(SocketMessage message)
        {
            var content = message.Content;

            if (content == "!help")
            {
                Console.WriteLine(
                    "[{0}] {1} {2}: {3}",
                    DateTime.Now,
                    message.Channel.Name,
                    message.Author.Username,
                    content);

                await message.Channel.SendMessageAsync("```\nBleep bloop. I only do !whois [name]\n```");

                return;
            }

            var whois = RegexWhois.Match(content);

            if (whois.Success)
            {
                Console.WriteLine(
                    "[{0}] {1} {2}: {3}",
                    DateTime.Now,
                    message.Channel.Name,
                    message.Author.Username,
                    content);

                var characterName = whois.Groups[1].Value;

                var requestUrl = string.Format("http://people.anarchy-online.com/character/bio/d/5/name/{0}/bio.xml?data_type=json", characterName);
                var result = Client.DownloadString(requestUrl);

                if (result == "null")
                {
                    await message.Channel.SendMessageAsync(string.Format("```\nBleep bloop. Couldn't find character '{0}'.\n```", characterName));
                }
                else
                {
                    var allData = (dynamic)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                    var mainData = allData[0];
                    var orgData = allData[1];
                    var updated = allData[2];

                    var headUrl = string.Format("http://cdn.funcom.com/billing_files/AO_shop/face/{0}.jpg", mainData.HEADID);
                    var headData = Client.DownloadData(headUrl);
                    var stream = new MemoryStream(headData);

                    var output = "```\n";

                    output +=
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
                        output +=
                            string.Format(
                                " {0} of {1}.",
                                orgData.RANK_TITLE,
                                orgData.NAME);
                    }

                    output = output.Replace("  ", " ");

                    output += string.Format(" (Updated {0}).\n```", updated);

                    await message.Channel.SendFileAsync(stream, "head.jpg", output);
                }

                return;
            }
        }
    }
}
