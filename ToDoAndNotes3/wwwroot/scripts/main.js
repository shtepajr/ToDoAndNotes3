document.addEventListener('DOMContentLoaded', function () {
    var burgerMenuToggle = document.getElementById('burger-menu-toggle');
    var sidebar = document.getElementById('sidebar');

    burgerMenuToggle.addEventListener('click', function () {
        sidebar.classList.toggle('sidebar-hide');
        checkWindowSize();
    });
});

// Hide main if (is on mobile screen) + (sidebar is shown)
function checkWindowSize() {
    const aside = document.getElementById('sidebar');
    const main = document.getElementById('main');

    if (window.innerWidth <= 768 && !aside.classList.contains('sidebar-hide')) {
        main.style.visibility = 'hidden';
        main.style.opacity = 0;
    }
    else {
        main.style.visibility = 'visible';
        main.style.opacity = 1;

    }
}

window.addEventListener('load', checkWindowSize);
window.addEventListener('resize', checkWindowSize);