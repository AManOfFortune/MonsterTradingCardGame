using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MonsterTradingCardGame.ServerLogic
{
    public class HttpRequest
    {
        public string Method = "";
        public string Location = "";
        public string? Authorization = null;
        public bool DoAction = true;
        public JObject Content = new ();

        public HttpRequest(string requestString)
        {
            if(requestString.Length > 0)
                ParseRequest(requestString);
        }

        private void ParseRequest(string httpRequest)
        {
            var reader = new StringReader(httpRequest);

            // Loops each line of the request and fills parsedRequest JObject
            for (string line = reader.ReadLine()!; line != null; line = reader.ReadLine()!)
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
                    case "Authorization":
                        Authorization = line.Split(": Basic ")[1];
                        break;
                }
            }

            // Content is located after two newline characters
            var content = httpRequest.Split(Environment.NewLine + Environment.NewLine);
            // Remove empty strings from array
            content = content.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            // If Split returned 2 entries, it means there exists regular JSON content sent by the client
            if (content.Length > 1)
            {
                var contentString = content[1];

                // If the first letter of the content is a "[", it means the content is an array and needs some additional syntax for JObject.Parse to work
                if (contentString[0] == '[')
                {
                    contentString = contentString.Insert(0, "{Array:");
                    contentString += "}";
                }
                // If the first letter is not a "{", we just assume the content to be a string and add it as such
                else if (contentString[0] != '{')
                {
                    contentString = contentString.Insert(0, "{String:");
                    contentString += "}";
                }

                Content = JObject.Parse(contentString);
            }

            // There might be more content after the location string, so we need to add that as content too
            // F. ex. "/users/altenhof" -> "altenhof" is the content

            // Isolate location string
            var firstLine = httpRequest.Split(Environment.NewLine)[0];
            var arrayOfThingsAfterLocation = firstLine.Split(" ")[1].Split(Location + "/");

            // If the array produced by splitting the location string by the Method + / has more elements than 1, add that as content
            // F. ex. "/users/altenhof", split by "/users/", results into "["/users/", "altenhof"]
            // F. ex. "/session", split by "/session/", results into "["/session"]
            if (arrayOfThingsAfterLocation.Length > 1)
                Content.Add("LocationParams", arrayOfThingsAfterLocation[1]);
        }
    }
}
