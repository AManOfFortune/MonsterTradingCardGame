using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Environment = System.Environment;

namespace MonsterTradingCardGame
{
    internal class HttpResponse
    {
        public int StatusCode;
        public JObject Body;
        public Dictionary<string, string>? Headers;

        public HttpResponse(int statusCode, JObject? body = null, Dictionary<string, string>? headers = null)
        {
            StatusCode = statusCode;
            Body = body ?? new JObject();
            Headers = headers;
        }

        // If response is sent (= cast to string) a correct response string is created as well
        public static implicit operator string(HttpResponse castMeToString) => castMeToString.ToString();

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

            if (Body.HasValues)
            {
                bodyLength = Body.ToString().Length;
                bodyString += Body;
            }

            return "HTTP/1.0 " + StatusCode + Environment.NewLine
                   + "Content-Length: " + bodyLength + Environment.NewLine
                   + "Content-Type: text/plain" + Environment.NewLine
                   + "Access-Control-Allow-Origin: *" + Environment.NewLine
                   + "Access-Control-Allow-Methods: POST, GET, OPTIONS" + Environment.NewLine
                   + "Access-Control-Allow-Headers: X-PINGOTHER, Content-Type" + Environment.NewLine
                   + additionalHeadersString
                   + bodyString
                   + Environment.NewLine + Environment.NewLine;
        }

        // Common status codes to make creating the correct response easier
        public static HttpResponse Ok => new (200);
        public static HttpResponse Created => new(201);
        public static HttpResponse BadRequest => new(400);
        public static HttpResponse Unauthorized => new(401);
        public static HttpResponse Forbidden => new(403);
        public static HttpResponse NotFound => new(404);
        public static HttpResponse MethodNotAllowed => new(405);
        public static HttpResponse InternalServerError => new(500);
        public static HttpResponse ServiceUnavailable => new(503);
    }
}
