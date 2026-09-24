document.addEventListener("DOMContentLoaded", () => {
    const toggle = document.querySelector(".nav-toggle");
    const navigation = document.querySelector("#main-navigation");

    if (!toggle || !navigation) {
        return;
    }

    toggle.addEventListener("click", () => {
        const isOpen = navigation.classList.toggle("is-open");
        toggle.setAttribute("aria-expanded", String(isOpen));
    });
});
