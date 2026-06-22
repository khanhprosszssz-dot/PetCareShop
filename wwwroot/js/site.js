document.addEventListener("DOMContentLoaded", function () {
    const dropdowns = document.querySelectorAll(".js-dropdown");

    if (dropdowns.length === 0) {
        return;
    }

    function closeAllDropdowns(exceptDropdown = null) {
        dropdowns.forEach(function (dropdown) {
            if (dropdown === exceptDropdown) {
                return;
            }

            dropdown.classList.remove("is-open");

            const button = dropdown.querySelector(
                ".js-dropdown-button"
            );

            if (button) {
                button.setAttribute(
                    "aria-expanded",
                    "false"
                );
            }
        });
    }

    dropdowns.forEach(function (dropdown) {
        const button = dropdown.querySelector(
            ".js-dropdown-button"
        );

        const menu = dropdown.querySelector(
            ".dropdown-menu"
        );

        if (!button || !menu) {
            return;
        }

        button.addEventListener("click", function (event) {
            event.preventDefault();
            event.stopPropagation();

            const isCurrentlyOpen =
                dropdown.classList.contains("is-open");

            closeAllDropdowns(dropdown);

            if (isCurrentlyOpen) {
                dropdown.classList.remove("is-open");

                button.setAttribute(
                    "aria-expanded",
                    "false"
                );
            } else {
                dropdown.classList.add("is-open");

                button.setAttribute(
                    "aria-expanded",
                    "true"
                );
            }
        });

        dropdown.addEventListener("click", function (event) {
            event.stopPropagation();
        });

        dropdown.addEventListener("mouseenter", function () {
            closeAllDropdowns(dropdown);
        });
    });

    document.addEventListener("click", function () {
        closeAllDropdowns();
    });

    document.addEventListener("keydown", function (event) {
        if (event.key !== "Escape") {
            return;
        }

        const openedDropdown =
            document.querySelector(
                ".js-dropdown.is-open"
            );

        closeAllDropdowns();

        if (openedDropdown) {
            const openedButton =
                openedDropdown.querySelector(
                    ".js-dropdown-button"
                );

            if (openedButton) {
                openedButton.focus();
            }
        }
    });
});