namespace AnarchyBot.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    public class Whois : HandlerBase
    {
        private WebClient webClient = new WebClient();

        public override string Command
        {
            get
            {
                return "whois";
            }
        }

        public override bool NeedsParameter
        {
            get
            {
                return true;
            }
        }

        public override void Process(string parameter, Action<string> sendResponse, Action<Stream, string, string> sendResponseFile)
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
                sendResponse(string.Format("Couldn't find character '{0}'.", parameter));

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

            // add the general character info to the response
            response +=
                string.Format(
                    "{0} '{1}' {2} is a{3} {4} {5} {6} {7}, level {8}.",
                    mainData.FIRSTNAME,
                    mainData.NAME,
                    mainData.LASTNAME,
                    mainData.SIDE == "Omni" ? "n" : "", // 'an', not 'a' :)
                    mainData.SIDE,
                    mainData.SEX,
                    mainData.BREED == "Nano" ? "Nanomage" : mainData.BREED, // might as well give them the proper name
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

            // add last updated and finish response
            response += string.Format(" (Updated {0}).", updated);
            
            // remove double spaces from response, this might happen if the character doesn't have a first / last name
            response = response.Replace("  ", " ").Trim();
            
            sendResponseFile(headStream, "head.jpg", response);
            
            // stop
            return;
        }
    }
}
