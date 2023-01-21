using NUnit.Framework;
using MonsterTradingCardGame.ServerLogic;
using MonsterTradingCardGame.RouteLogic;
using MonsterTradingCardGame.StateLogic;
using Newtonsoft.Json.Linq;

namespace Testing;

[TestFixture]
public class RouterTests
{
    [Test]
    public void TestInvalidMethodRouting()
    {
        var httpString = @"CLEAR /stats HTTP/1.1
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

        var router = new Router();

        var response = router.Route(parsedString);
        
        Assert.AreEqual(response.StatusCode, 405);
    }
    
    [Test]
    public void TestInvalidLocationRouting()
    {
        var httpString = @"GET /randomRoute HTTP/1.1
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

        var router = new Router();

        var response = router.Route(parsedString);
        
        Assert.AreEqual(response.StatusCode, 404);
    }
    
    [Test]
    public void TestPreflightRequestRouting()
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

        var router = new Router();

        var response = router.Route(parsedString);
        
        Assert.AreEqual(response.StatusCode, 200);
    }
    
    [Test]
    public void TestMissingAuthorizationRouting()
    {
        var httpString = @"GET /stats HTTP/1.1
Host: localhost:10001
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0
Accept: */*
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Referer: http://127.0.0.1:5500/
Origin: http://127.0.0.1:5500
DNT: 1
Connection: keep-alive
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: cors
Sec-Fetch-Site: cross-site";

        var parsedString = new HttpRequest(httpString);

        var router = new Router();

        var response = router.Route(parsedString);
        
        Assert.AreEqual(response.StatusCode, 401);
    }
    
    [Test]
    public void TestInvalidTokenRouting()
    {
        var httpString = @"GET /stats HTTP/1.1
Host: localhost:10001
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/109.0
Accept: */*
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Referer: http://127.0.0.1:5500/
Authorization: Basic user-mtcgToken
Origin: http://127.0.0.1:5500
DNT: 1
Connection: keep-alive
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: cors
Sec-Fetch-Site: cross-site";

        UserManager.Instance.NewUserLogin("admin-mtcgToken", 0);

        var parsedString = new HttpRequest(httpString);

        var router = new Router();

        var response = router.Route(parsedString);
        
        Assert.AreEqual(response.StatusCode, 401);
    }
}