using NUnit.Framework;
using MonsterTradingCardGame.ServerLogic;
using Newtonsoft.Json.Linq;

namespace Testing;

[TestFixture]
public class HttpRequestTests
{
    [Test]
    public void TestMethodParsing()
    {
        var httpString = @"GET /stats HTTP/1.1
Host: localhost:10001
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0
Accept: */*
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Referer: http://127.0.0.1:5500/
Authorization: Basic admin-mtcgToken
Origin: http://127.0.0.1:5500
DNT: 1
Connection: keep-alive
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: cors
Sec-Fetch-Site: cross-site";

        var parsedString = new HttpRequest(httpString);

        Assert.True(parsedString.DoAction);
        Assert.AreEqual(parsedString.Method, "GET");
    }
    
    [Test]
    public void TestPreflightRequestParsing()
    {
        var httpString = @"OPTIONS /stats HTTP/1.1
Host: localhost:10001
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0
Accept: */*
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Access-Control-Request-Method: GET
Access-Control-Request-Headers: authorization
Referer: http://127.0.0.1:5500/
Origin: http://127.0.0.1:5500
DNT: 1
Connection: keep-alive
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: cors
Sec-Fetch-Site: cross-site
Cache-Control: max-age=0";

        var parsedString = new HttpRequest(httpString);

        Assert.False(parsedString.DoAction);
        Assert.AreEqual(parsedString.Method, "GET");
        Assert.AreEqual(parsedString.Location, "/stats");
    }
    
    [Test]
    public void TestSimpleLocationParsing()
    {
        var httpString = @"GET /stats HTTP/1.1
            Host: localhost:10001
            User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0
            Accept: */*
            Accept-Language: en-US,en;q=0.5
            Accept-Encoding: gzip, deflate, br
            Referer: http://127.0.0.1:5500/
            Authorization: Basic admin-mtcgToken
            Origin: http://127.0.0.1:5500
            DNT: 1
            Connection: keep-alive
            Sec-Fetch-Dest: empty
            Sec-Fetch-Mode: cors
            Sec-Fetch-Site: cross-site";

        var parsedString = new HttpRequest(httpString);

        Assert.AreEqual(parsedString.Location, "/stats");
    }
    
    [Test]
    public void TestComplexLocationParsing()
    {
        var httpString = @"GET /stats/altenhof?password=unsecure HTTP/1.1
Host: localhost:10001
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0
Accept: */*
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Referer: http://127.0.0.1:5500/
Authorization: Basic admin-mtcgToken
Origin: http://127.0.0.1:5500
DNT: 1
Connection: keep-alive
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: cors
Sec-Fetch-Site: cross-site";

        var parsedString = new HttpRequest(httpString);

        Assert.AreEqual(parsedString.Location, "/stats");
    }
    
    [Test]
    public void TestAuthorizationParsing()
    {
        var httpString = @"GET /stats/altenhof?password=unsecure HTTP/1.1
Host: localhost:10001
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0
Accept: */*
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Referer: http://127.0.0.1:5500/
Authorization: Basic admin-mtcgToken
Origin: http://127.0.0.1:5500
DNT: 1
Connection: keep-alive
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: cors
Sec-Fetch-Site: cross-site";

        var parsedString = new HttpRequest(httpString);
        
        Assert.AreEqual(parsedString.Authorization, "admin-mtcgToken");
    }
    
    [Test]
    public void TestJsonContentParsing()
    {
        var httpString = @"POST /sessions HTTP/1.1
Host: localhost:10001
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0
Accept: */*
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Referer: http://127.0.0.1:5500/
Content-Type: application/json
Content-Length: 37
Origin: http://127.0.0.1:5500
DNT: 1
Connection: keep-alive
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: cors
Sec-Fetch-Site: cross-site

{'Username':'test','Password':'test'}";

        var parsedString = new HttpRequest(httpString);
        var correctContent = JObject.Parse("{\"Username\":\"test\",\"Password\":\"test\"}");
        
        Assert.True(JObject.DeepEquals(parsedString.Content, correctContent));
    }
    
    [Test]
    public void TestArrayContentParsing()
    {
        var httpString = @"POST /sessions HTTP/1.1
Host: localhost:10001
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0
Accept: */*
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Referer: http://127.0.0.1:5500/
Content-Type: application/json
Content-Length: 37
Origin: http://127.0.0.1:5500
DNT: 1
Connection: keep-alive
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: cors
Sec-Fetch-Site: cross-site

['Username', 'test']";

        var parsedString = new HttpRequest(httpString);
        var correctContent = JObject.Parse("{\"Array\":[\"Username\",\"test\"]}");
        
        Assert.True(JObject.DeepEquals(parsedString.Content, correctContent));
    }
    
    [Test]
    public void TestStringContentParsing()
    {
        var httpString = @"POST /sessions HTTP/1.1
Host: localhost:10001
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0
Accept: */*
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Referer: http://127.0.0.1:5500/
Content-Type: application/json
Content-Length: 37
Origin: http://127.0.0.1:5500
DNT: 1
Connection: keep-alive
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: cors
Sec-Fetch-Site: cross-site

'Username'";

        var parsedString = new HttpRequest(httpString);
        var correctContent = JObject.Parse("{\"String\":\"Username\"}");
        
        Assert.True(JObject.DeepEquals(parsedString.Content, correctContent));
    }
    
    [Test]
    public void TestParameterContentParsing()
    {
        var httpString = @"GET /stats/altenhof?password=unsecure HTTP/1.1
Host: localhost:10001
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0
Accept: */*
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Referer: http://127.0.0.1:5500/
Authorization: Basic admin-mtcgToken
Origin: http://127.0.0.1:5500
DNT: 1
Connection: keep-alive
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: cors
Sec-Fetch-Site: cross-site";

        var parsedString = new HttpRequest(httpString);
        var correctContent = JObject.Parse("{\"LocationParams\":\"altenhof?password=unsecure\"}");
        
        Assert.True(JObject.DeepEquals(parsedString.Content, correctContent));
    }
}