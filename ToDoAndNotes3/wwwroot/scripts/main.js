document.addEventListener('DOMContentLoaded', function () {
    var burgerMenuToggle = document.getElementById('burger-menu-toggle');
    var sidebar = document.getElementById('sidebar');

    burgerMenuToggle.addEventListener('click', function () {
        sidebar.classList.toggle('sidebar-hide');
        checkWindowSize();
    });

    // Testing
    LoadTestProjects();
    LoadTestTasks();
    LoadTestNotes();
    LoadTestTasksWithNotes();
});

// Hide main if (is on mobile screen) + (sidebar is shown)
function checkWindowSize() {
    const aside = document.getElementById('sidebar');
    const main = document.getElementById('main');

    if (window.innerWidth <= 600 && !aside.classList.contains('sidebar-hide')) {
        main.style.opacity = 0;
        setTimeout(() => {
            main.style.display = 'none';
        }, 200);       
    }
    else {
        main.style.opacity = 1;
        setTimeout(() => {
            main.style.display = 'block';
        }, 200);  
    }
}

window.addEventListener('load', checkWindowSize);
window.addEventListener('resize', checkWindowSize);


// For testing
function LoadTestProjects(){
    var projectsElem = document.querySelector('.js-projects-test');
    let htmlChild = `
                <li class="nav-link">
                <a>
                    English
                    <span class="material-symbols-outlined md-24 black">
                        more_horiz
                    </span>
                </a>
                </li>
    `;

    let innerHTML = '';
    for (var i = 0; i < 30; i++) {
        innerHTML += htmlChild;
    }
    projectsElem.innerHTML = innerHTML;
}
function LoadTestTasks() {
    var projectsElem = document.querySelector('.js-tasks-test');
    let htmlChild = `
            <div class="task">Hello task</div>
    `;

    let innerHTML = '';
    for (var i = 0; i < 60; i++) {
        innerHTML += htmlChild;
    }
    projectsElem.innerHTML = innerHTML;
}
function LoadTestNotes() {
    var projectsElem = document.querySelector('.js-notes-test');
    let htmlChild = `
            <div class="note">Hello note</div>
    `;

    let innerHTML = '';
    for (var i = 0; i < 60; i++) {
        innerHTML += htmlChild;
    }
    projectsElem.innerHTML = innerHTML;
}
function LoadTestTasksWithNotes() {
    var projectsElem = document.querySelector('.js-tasks-with-notes-test');
    let htmlChild = `
            <div class="task">Hello task</div>
            <div class="note">Hello note</div>
    `;

    let innerHTML = '';
    for (var i = 0; i < 60; i++) {
        innerHTML += htmlChild;
    }
    projectsElem.innerHTML = innerHTML;
}