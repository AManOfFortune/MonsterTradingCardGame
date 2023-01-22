using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Environment = System.Environment;

namespace MonsterTradingCardGame.ServerLogic
{
    public class HttpResponse
    {
        public int StatusCode;
        public string StatusMessage { get; private set; }
        public JObject Body;
        public Dictionary<string, string>? Headers;
        public bool KeepConnectionAlive;

        public HttpResponse(int statusCode,  string statusMessage, JObject? body = null, Dictionary<string, string>? headers = null)
        {
            StatusCode = statusCode;
            StatusMessage = statusMessage;
            Body = body ?? new JObject();
            Headers = headers;
            KeepConnectionAlive = false;
        }

        // If response is sent (= cast to string) a correct response string is created as well
        public static implicit operator string(HttpResponse castMeToString) => castMeToString.ToString();

        public HttpResponse WithStatusMessage(string message)
        {
            StatusMessage = message;
            return this;
        }
        
        public HttpResponse WithFileContent(string fileContent)
        {
            Body.Add("FileContent", fileContent);
            return this;
        }
        
        // Creates a valid http response string
        public override string ToString()
        {
            var additionalHeadersString = "";
            string bodyString;
            string contentType;

            if (Headers != null)
            {
                additionalHeadersString += Environment.NewLine;

                foreach (var header in Headers)
                {
                    additionalHeadersString += header.Key + ": " + header.Value + Environment.NewLine;
                }
            }

            if(KeepConnectionAlive)
            {
                additionalHeadersString += "Connection: keep-alive" + Environment.NewLine;
            }
            
            if(!Body.ContainsKey("Message"))
                Body.Add("Message", StatusMessage);
            
            // TODO: Implement a better way to differentiate files from json content
            switch (StatusMessage)
            {
                case "html":
                    contentType = "text/html";
                    bodyString = Body["FileContent"]!.ToString();
                    break;
                case "css":
                    contentType = "text/css";
                    bodyString = Body["FileContent"]!.ToString();
                    break;
                case "js":
                    contentType = "text/javascript";
                    bodyString = Body["FileContent"]!.ToString();
                    break;
                default:
                    contentType = "application/json";
                    bodyString = Body.ToString();
                    break;
            }

            int bodyLength = bodyString.Length;

            return "HTTP/1.0 " + StatusCode + Environment.NewLine
                   + "Content-Length: " + bodyLength + Environment.NewLine
                   + "Content-Type: " + contentType + Environment.NewLine
                   + "Access-Control-Allow-Origin: *" + Environment.NewLine
                   + "Access-Control-Allow-Methods: POST, GET, OPTIONS, PUT, DELETE" + Environment.NewLine
                   + "Access-Control-Allow-Headers: X-PINGOTHER, Content-Type, Authorization" + Environment.NewLine
                   + additionalHeadersString + Environment.NewLine
                   + bodyString
                   + Environment.NewLine + Environment.NewLine;
        }

        // Common status codes to make creating the correct response easier
        public static HttpResponse Ok => new (200, "Success!");
        public static HttpResponse Created => new(201, "Successfully created!");
        public static HttpResponse BadRequest => new(400, "Request has invalid/missing parameters!");
        public static HttpResponse Unauthorized => new(401, "Request requires Authorization!");
        public static HttpResponse Forbidden => new(403, "Action not allowed!");
        public static HttpResponse NotFound => new(404, "Resource not found!");
        public static HttpResponse MethodNotAllowed => new(405, "Method not allowed!");
        public static HttpResponse MediaTypeNotSupported => new(415, "File type not supported!");
        public static HttpResponse InternalServerError => new(500, "Oops! Something went wrong on our end, please try again later.");
        public static HttpResponse OkWithHtml => new(200, "html");
        public static HttpResponse OkWithCss => new(200, "css");
        public static HttpResponse OkWithJs => new(200, "js");
    }
}
