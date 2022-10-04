async function queryData(method, location, params) {
    const url = "http://localhost:10001" + location
    
    await fetch(
        url, 
        {
            method: method,
            cache: 'no-cache',
            headers: {
                'Content-Type': 'application/json'
            },
            body: params
        }
    )
    .then((response) => response.text())
    .then(text => {
        try {
            console.log(text)
        }
        catch(err) {
            console.error(err)
        }
    })
}

document.querySelector("#register").addEventListener("click", () => {
    queryData("POST", "/users", JSON.stringify({username: "altenhof", password: "philipp"}))
})

document.querySelector("#login").addEventListener("click", () => {
    queryData("GET", "/users/altenhof")
})

document.querySelector("#invalid").addEventListener("click", () => {
    queryData("POST", "/options")
})