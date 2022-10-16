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

// document.querySelector("#register").addEventListener("click", () => {
//     queryData("POST", "/users", JSON.stringify({username: "test", password: "test"}))
// })

document.querySelector("#loginModal form").addEventListener("submit", async (e) => {
    e.preventDefault()

    const username = document.querySelector("#username").value
    const password = document.querySelector("#password").value

    console.log("Username: " + username + " Password: " + password)

    await queryData("POST", "/sessions", JSON.stringify({username: username, password: password}))
})