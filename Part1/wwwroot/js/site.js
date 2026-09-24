<script>
    function goToLogin() {
      let role = document.getElementById("role").value

    if (role === "employee") {
        window.location.href = "Home/Login"
        }
    else if (role === "donor") {
        window.location.href = "/login"
        }
    else {
        alert("Please select a role.")
        }
    }


</script>
