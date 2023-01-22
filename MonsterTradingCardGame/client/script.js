var authToken = "admin-mtcgToken"
var loggedInUsername = "admin"
var ownedCards = {}
var openTrades = {}

async function queryData(method, location, params, customHeaders = {}) {
    const url = "http://localhost:10001" + location
    
    if(params != null) { customHeaders = Object.assign(customHeaders, {"Content-Type": "application/json"}) }

    var responseJSON;

    await fetch(
        url, 
        {
            method: method,
            cache: 'no-cache',
            headers: customHeaders,
            body: params
        }
    )
    .then(async (response) => {
        var statusCode = response.status
        var json = await response.json()

        showResponse(statusCode, json)
        
        return json
    })
    .then(text => {
        try {
            console.log(text)
        
            if("authToken" in text) {
                authToken = text["authToken"]
                console.log("New authtoken: " + authToken)
            }
            
            responseJSON = text;
        }
        catch(err) {
            console.error(err)
        }
    })

    return responseJSON;
}

async function queryContinousData(method, location, params, customHeaders = {}) 
{
    const url = "http://localhost:10001" + location
    
    if(params != null) { customHeaders = Object.assign(customHeaders, {"Content-Type": "application/json"}) }

    var responseJSON;

    await fetch(
        url, 
        {
            method: method,
            cache: 'no-cache',
            headers: customHeaders,
            body: params
        }
    )
    .then(async (response) => {

        var printable = {}

        try {
            printable = await response.json()
        }
        catch(err) {}

        console.log(printable)

        showResponse(response.status, printable)

        if(printable.Message == "Searching for an opponent...")
        {
            setTimeout(() => {
                queryContinousData(method, location, params, customHeaders)
            }, 3000)
        }
    })

    return responseJSON;
}

function showResponse(statusCode, responseObject) {
    document.querySelector("#statusCode").value = statusCode;
    document.querySelector("#responseObject").value = JSON.stringify(responseObject);
}

function getUsernameAndPassword() {
    const username = document.querySelector("#username").value
    const password = document.querySelector("#password").value

    console.log("Username: " + username + " Password: " + password)

    loggedInUsername = username

    return {Username: username, Password: password}
}

function GUID() {
    const array = new Uint8Array(16);
    window.crypto.getRandomValues(array);
    array[6] = (array[6] & 0x0f) | 0x40;
    array[8] = (array[8] & 0x3f) | 0x80;
    return Array.from(array, dec => ("0" + dec.toString(16)).substr(-2)).join("");
  }

document.querySelector("#login").addEventListener("click", async (e) => {
    e.preventDefault()

    await queryData("POST", "/sessions", JSON.stringify(getUsernameAndPassword()))
})

document.querySelector("#register").addEventListener("click", async (e) => {
    e.preventDefault()

    await queryData("POST", "/users", JSON.stringify(getUsernameAndPassword()))
})

document.querySelector("#getUserData").addEventListener("click", async (e) => {
    e.preventDefault()

    await queryData("GET", "/users/" + loggedInUsername, null, { "Authorization": "Basic " + authToken })
})

document.querySelector("#getStats").addEventListener("click", async (e) => {
    e.preventDefault()

    await queryData("GET", "/stats", null, { "Authorization": "Basic " + authToken })
})

document.querySelector("#updateUserData").addEventListener("click", async (e) => {
    e.preventDefault()

    await queryData("PUT", "/users/" + loggedInUsername, JSON.stringify({Name: loggedInUsername, Bio: "Me vibin...", Image: ":D"}), { "Authorization": "Basic " + authToken })
})

document.querySelector("#getScoreboard").addEventListener("click", async (e) => {
    e.preventDefault()

    await queryData("GET", "/score", null, { "Authorization": "Basic " + authToken })
})

document.querySelector("#addPackages").addEventListener("click", async (e) => {
    e.preventDefault()

    await queryData("POST", "/packages", JSON.stringify([
        {Id: GUID(), Name: "WaterGoblin", Damage: 10.0}, 
        {Id: GUID(), Name: "Dragon", Damage: 50.0}, 
        {Id: GUID(), Name: "WaterSpell", Damage: 20.0}, 
        {Id: GUID(), Name: "Ork", Damage: 45.0}, 
        {Id: GUID(), Name: "FireSpell", Damage: 25.0}
    ]),
    { "Authorization": "Basic admin-mtcgToken" })
})

document.querySelector("#buyPackage").addEventListener("click", async (e) => {
    e.preventDefault()

    await queryData("POST", "/transactions/packages", null, { "Authorization": "Basic " + authToken })
})

document.querySelector("#getOwnedCards").addEventListener("click", async (e) => {
    e.preventDefault()

    ownedCards = await queryData("GET", "/cards", null, { "Authorization": "Basic " + authToken })
})

document.querySelector("#getDeck").addEventListener("click", async (e) => {
    e.preventDefault()

    await queryData("GET", "/deck", null, { "Authorization": "Basic " + authToken })
})

document.querySelector("#getDeckPlain").addEventListener("click", async (e) => {
    e.preventDefault()

    await queryData("GET", "/deck?format=plain", null, { "Authorization": "Basic " + authToken })
})

document.querySelector("#configureDeck").addEventListener("click", async (e) => {
    e.preventDefault()

    var body = []

    while (body.length < 4) 
    {
        const index = Math.floor(Math.random() * ownedCards.cards.length);
  
        // Check if the index has already been added to the body array
        if (!body.includes(ownedCards.cards[index].Id)) {
            // If the index has not been added, add it to the body array
            body.push(ownedCards.cards[index].Id)
        }
    }

    console.log(body)

    await queryData("PUT", "/deck", JSON.stringify(body), { "Authorization": "Basic " + authToken })
})

document.querySelector("#getTrades").addEventListener("click", async (e) => {
    e.preventDefault()

    openTrades = await queryData("GET", "/tradings", null, { "Authorization": "Basic " + authToken })
})

document.querySelector("#createTrade").addEventListener("click", async (e) => {
    e.preventDefault()

    var card_id

    while (true) {
        const index = Math.floor(Math.random() * ownedCards.cards.length);
    
        if (ownedCards.cards[index].PosInDeck == 0) {
            card_id = ownedCards.cards[index].Id
            break
        }
    }

    var body = {
        Id: GUID(),
        CardToTrade: card_id,
        Type: "monster",
        MinimumDamage: 15
    }

    console.log(body)

    await queryData("POST", "/tradings", JSON.stringify(body), { "Authorization": "Basic " + authToken })
})

document.querySelector("#buyTrade").addEventListener("click", async (e) => {
    e.preventDefault()

    var card_id

    var offeredCard

    while (true) {
        const index = Math.floor(Math.random() * ownedCards.cards.length);
    
        if (ownedCards.cards[index].PosInDeck == 0) {
            card_id = ownedCards.cards[index].Id
            offeredCard = ownedCards.cards[index]
            break
        }
    }

    const random = Math.floor(Math.random() * openTrades.Tradingdeals.length)

    var trade_id = openTrades.Tradingdeals[random].Id

    console.log("Wanna buy that:")
    console.log(openTrades.Tradingdeals[random])
    console.log("With this:")
    console.log(offeredCard)

    await queryData("POST", "/tradings/" + trade_id, "\"" + card_id + "\"", { "Authorization": "Basic " + authToken })
})

document.querySelector("#deleteTrade").addEventListener("click", async (e) => {
    e.preventDefault()

    const random = Math.floor(Math.random() * openTrades.Tradingdeals.length)

    var trade_id = openTrades.Tradingdeals[random].Id

    console.log("Delete that:")
    console.log(openTrades.Tradingdeals[random])

    await queryData("DELETE", "/tradings/" + trade_id, null, { "Authorization": "Basic " + authToken })
})

document.querySelector("#startBattle").addEventListener("click", async (e) => {
    e.preventDefault()

    await queryContinousData("POST", "/battles", null, { "Authorization": "Basic " + authToken })
})