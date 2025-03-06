document.addEventListener("DOMContentLoaded", function () {
    let navLinks = document.querySelectorAll("#navbar ul > li");

    // Lấy URL hiện tại
    let currentUrl = window.location.pathname.toLowerCase();

    navLinks.forEach(function (li) {
        let link = li.querySelector("a");
        if (link) {
            let linkUrl = link.getAttribute("href").toLowerCase();
            if (currentUrl.includes(linkUrl)) {
                li.classList.add("active");
            }
        }
    });
});