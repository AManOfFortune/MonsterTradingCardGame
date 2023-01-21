// Function to show a modal
function showModal(modalID, closeID) {
    const modal = document.getElementById(modalID);

    // Get all elements with a given class
    const span = document.getElementsByClassName(closeID);

    // When the user clicks on the button, open the modal
    modal.style.display = "block";

    // When the user clicks on an element with the given class, close the modal
    for(let i = 0; i < span.length; i++) {
        span[i].onclick = function() {
            modal.style.display = "none";
        }
    }

    // When the user clicks anywhere outside of the modal, close it
    window.onclick = function(event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    }
}

document.querySelector("#openLogin").addEventListener("click", () => {
    showModal("loginModal", "close_login");
})