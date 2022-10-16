using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame
{
    internal class HttpRequest
    {
        public string Method;
        public string Location;
        public bool DoAction = true;
        public JObject? Content;

        //HttpRequest(string method = "", string location = "", bool doAction = true, JObject? content = null)
        //{
        //    Method = method;
        //    Location = location;
        //    DoAction = doAction;
        //    Content = content;
        //}

        public HttpRequest(string requestString)
        {
            ParseRequest(requestString);
        }

        private void ParseRequest(string httpRequest)
        {
            var reader = new StringReader(httpRequest);

            // Loops each line of the request and fills parsedRequest JObject
            for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                // First line of request contains Method and Location
                if (line.Contains("HTTP"))
                {
                    Method = line.Split(" ")[0];
                    Location = "/" + line.Split(" ")[1].Split("/")[1];
                    continue;
                }

                // All other lines are either Headers or Content, each header we are interested in we save in the parsedRequest JObject
                switch (line.Split(": ")[0])
                {
                    case "Access-Control-Request-Method":
                        DoAction = false;
                        Method = line.Split(": ")[1];
                        break;
                }
            }

            // Content is located after two newline characters
            var content = httpRequest.Split(Environment.NewLine + Environment.NewLine);
            // Remove empty strings from array
            content = content.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            // If Content array has only one entry (split could not find two new lines), the request either has no content, or the "content" is placed after the initial location
            // F. ex. "/users/altenhof" -> "altenhof" is the content
            if (content.Length <= 1)
            {
                // Isolate location string
                var firstLine = httpRequest.Split(Environment.NewLine)[0];
                var arrayOfThingsAfterLocation = firstLine.Split(" ")[1].Split(Method + "/");

                // If the array produced by splitting the location string by the Method + / has more elements than 1, add that as content
                // F. ex. "/users/altenhof", split by "/users/", results into "["/users/", "altenhof"]
                // F. ex. "/session", split by "/session/", results into "["/session"]
                if (arrayOfThingsAfterLocation.Length > 1)
                    Content = new JObject { { "locationParams", arrayOfThingsAfterLocation[1] } };
                // Otherwise, request contains no body
                else
                    Content = null;
            }
            // If content has min. 2 entries, its regular JSON sent by the client
            else
            {
                Content = JObject.Parse(content[1]);
            }
        }
    }
}
