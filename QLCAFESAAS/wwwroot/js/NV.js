document.addEventListener("DOMContentLoaded", function () {
    var confirmLink = document.getElementById("confirm-link");
    var logoutForm = document.getElementById("logout-form");
    var overlay = document.getElementById("overlay");
    var cancelLogoutBtn = document.getElementById("cancel-logout");
    var confirmLogoutBtn = document.getElementById("confirm-logout");
    var errorMessage = document.getElementById("error-message");

    // Khi bấm vào "Tên người dùng", hiển thị form
    confirmLink.addEventListener("click", function (event) {
        event.preventDefault();
        logoutForm.style.display = "block";
        overlay.style.display = "block";
    });

    // Khi bấm "Hủy", ẩn form
    cancelLogoutBtn.addEventListener("click", function () {
        logoutForm.style.display = "none";
        overlay.style.display = "none";
        errorMessage.style.display = "none";
    });

    // Xác nhận mật khẩu
    confirmLogoutBtn.addEventListener("click", function () {
        var passwordInput = document.getElementById("confirm-input").value;

        if (!passwordInput) {
            errorMessage.textContent = "Vui lòng nhập mật khẩu!";
            errorMessage.style.display = "block";
            return;
        }

        fetch("/Account/ConfirmLogout", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ password: passwordInput })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    window.location.href = data.redirectUrl; // Chuyển đến trang chủ
                } else {
                    errorMessage.textContent = data.message;
                    errorMessage.style.display = "block";
                }
            })
            .catch(error => console.error("Lỗi:", error));
    });
});
