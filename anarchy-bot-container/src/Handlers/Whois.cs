﻿namespace AnarchyBot.Handlers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    // whois handler
    public class Whois : HandlerBase
    {
        // webclient for retrieving foshizzle
        private HttpClient webClient = new HttpClient();

        // command name
        public override string Command => "whois";

        // we need a parameter (the character name)
        protected override bool NeedsParameter => true;

        public override string HelpText => "[name] Get character info";

        private async Task ProcessDimension(HandlerRequest request, int dimension)
        {
            var parameter = request.Parameter;

            // generate character info request url, json please
            var requestUrl = $"http://people.anarchy-online.com/character/bio/d/{dimension}/name/{parameter}/bio.xml?data_type=json";
            // get data using webclient
            var result = await this.webClient.GetStringAsync(requestUrl);

            if (result == "null")
            {
                // the web api returns the text 'null' if it can't find the character you requested
                // let the user know they've done something silly
                await this.SendResponse(
                    request,
                    $"Dimension {dimension}: Couldn't find character '{parameter}'.");

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
            var headUrl = $"http://cdn.funcom.com/billing_files/AO_shop/face/{mainData.HEADID}.jpg";
            // download the jpeg
            var headData = await this.webClient.GetByteArrayAsync(headUrl);
            // copy it into a memory stream
            var headStream = new MemoryStream(headData);

            var alienLevel = (int)mainData.ALIENLEVEL;

            // add the general character info to the response
            var response = $"Dimension {dimension}: ";

            response +=
                string.Format(
                    "{0} '{1}' {2} is a{3} {4} {5} {6} {7}, level {8} ({9}).",
                    mainData.FIRSTNAME,
                    mainData.NAME,
                    mainData.LASTNAME,
                    mainData.SIDE == "Omni" ? "n" : "", // 'an', not 'a' :)
                    mainData.SIDE,
                    mainData.SEX,
                    mainData.BREED == "Nano" ? "Nanomage" : mainData.BREED, // might as well give them the proper name
                    mainData.PROF,
                    mainData.LEVELX,
                    mainData.PROFNAME);

            if (alienLevel > 0)
            {
                response +=
                    string.Format(
                        " AI level {0} ({1}).",
                        alienLevel,
                        mainData.RANK_name);
            }

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

            // add last updated and finish response
            response += $" Last updated {updated}.";

            // remove double spaces from response, this might happen if the character doesn't have a first / last name
            response = response.Replace("  ", " ").Trim();

            // send the response
            await this.SendResponseFile(
                request,
                headStream,
                "head.jpg",
                response);
        }

        // process method
        protected override async Task Process(HandlerRequest request)
        {
            await this.ProcessDimension(request, 5);
            await this.ProcessDimension(request, 6);

            // stop
            return;
        }
    }
}
