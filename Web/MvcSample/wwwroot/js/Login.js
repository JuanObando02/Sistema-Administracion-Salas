document.getElementById("togglePassword").addEventListener("click", function () {
    const input = document.getElementById("passwordField");
    const isPassword = input.type === "password";
    input.type = isPassword ? "text" : "password";
});