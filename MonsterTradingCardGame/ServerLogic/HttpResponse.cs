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
        
        // Creates a valid http response string
        public override string ToString()
        {
            var additionalHeadersString = "";
            var bodyLength = 0;
            var bodyString = Environment.NewLine;

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

            Body.Add("Message", StatusMessage);

            bodyLength = Body.ToString().Length;
            bodyString += Body;

            return "HTTP/1.0 " + StatusCode + Environment.NewLine
                   + "Content-Length: " + bodyLength + Environment.NewLine
                   + "Content-Type: text/plain" + Environment.NewLine
                   + "Access-Control-Allow-Origin: *" + Environment.NewLine
                   + "Access-Control-Allow-Methods: POST, GET, OPTIONS, PUT, DELETE" + Environment.NewLine
                   + "Access-Control-Allow-Headers: X-PINGOTHER, Content-Type, Authorization" + Environment.NewLine
                   + additionalHeadersString
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
        public static HttpResponse InternalServerError => new(500, "Oops! Something went wrong on our end, please try again later.");
    }
}
