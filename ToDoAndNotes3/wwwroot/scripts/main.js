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
    //LoadTestTasksWithNotes();
});

window.addEventListener('load', checkWindowSize);
window.addEventListener('resize', checkWindowSize);

window.onclick = toggleDropdown;

function checkWindowSize() {
    const aside = document.getElementById('sidebar');
    const main = document.getElementById('main');

    // Hide main if (is on "mobile"" screen) + (sidebar is shown)
    if (window.innerWidth <= 635 && !aside.classList.contains('sidebar-hide')) {
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
function toggleDropdown(event) {
    let dropdownContentElems = document.getElementsByClassName("dropdown-content");
  
    if (event.target.classList.contains('dropdown-btn-ellipsis')) {
        for (let i = 0; i < dropdownContentElems.length; i++) {
            var dropdownContentElem = dropdownContentElems[i];
            // show dropdown-content if its button was clicked || hide if it was already shown
            if (dropdownContentElem.id === event.target.id) {
                dropdownContentElem.classList.toggle('show');
            }
            else { // hide previous dropdown-content
                dropdownContentElem.classList.remove('show');
            }
        }
    } // hide if click on other elements
    else {
        for (let i = 0; i < dropdownContentElems.length; i++) {
            dropdownContentElems[i].classList.remove('show');
        }
    }
    event.stopPropagation();  // Stop the event from propagating up the DOM tree
}


// Testing
function generateUniqueId() {
    const timestamp = new Date().getTime();
    const randomString = Math.random().toString(36).substring(2, 8);
    return `${timestamp}-${randomString}`;
}
function LoadTestProjects(){
    var projectsElem = document.querySelector('.js-projects-test');

    const items = [];
    items.length = 30;

    for (var i = 0; i < 30; i++) {

        const uniqueId = generateUniqueId();
        const dropdownContentElem = document.createElement('div');
        dropdownContentElem.className = 'dropdown-content';
        dropdownContentElem.innerHTML = `
                    <a href="#Edit">Edit</a>
                    <a href="#Remove">Remove</a>
        `;
        dropdownContentElem.id = uniqueId;


        const navElem = document.createElement('li');
        navElem.className = 'nav-link dropdown';
        navElem.innerHTML = `
                <a>
                    <div class="nowrap-ellipsis">Project: ${dropdownContentElem.id}</div>
                    <span class="material-symbols-outlined md-24 black dropdown-btn-ellipsis">
                        more_horiz
                    </span>
                </a>
        `;

        const dropdownBtnElem = navElem.getElementsByClassName('dropdown-btn-ellipsis')[0];
        dropdownBtnElem.id = uniqueId;

        dropdownBtnElem.addEventListener('click', (event) => {
            toggleDropdown(event);       
        });

        navElem.appendChild(dropdownContentElem);
        projectsElem.appendChild(navElem);
    }
}
function LoadTestTasks() {
    var projectsElem = document.querySelector('.js-tasks-test');

    const items = [];
    items.length = 30;

    for (var i = 0; i < 30; i++) {

        const uniqueId = generateUniqueId();
        const dropdownContentElem = document.createElement('div');
        dropdownContentElem.className = 'dropdown-content';
        dropdownContentElem.innerHTML = `
                    <a href="#Edit">Edit</a>
                    <a href="#Remove">Remove</a>
        `;
        dropdownContentElem.id = uniqueId;


        const navElem = document.createElement('li');
        navElem.className = 'task nav-link dropdown';
        navElem.innerHTML = `
                        <a>
                            <span class="material-symbols-outlined md-24 is-done-btn">
                                radio_button_unchecked
                            </span>
                            <div class="task-content">
                                <div class="nowrap-ellipsis">Task ${dropdownContentElem.id}</div>
                                <div class="tools-row">
                                    <div class="d-flex min-width-0">
                                        <span class="material-symbols-outlined md-20 black">
                                            date_range
                                        </span>
                                        <div class="nowrap-ellipsis">
                                            6 nov
                                        </div>
                                    </div>
                                    <div class="d-flex">
                                        <span class="material-symbols-outlined md-20 black">
                                            label
                                        </span>
                                        <div class="nowrap-ellipsis">
                                            Dev
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <span class="material-symbols-outlined md-24 black dropdown-btn-ellipsis">
                                more_horiz
                            </span>
                        </a>
        `;

        const dropdownBtnElem = navElem.getElementsByClassName('dropdown-btn-ellipsis')[0];
        dropdownBtnElem.id = uniqueId;

        dropdownBtnElem.addEventListener('click', (event) => {
            toggleDropdown(event);
        });

        navElem.appendChild(dropdownContentElem);
        projectsElem.appendChild(navElem);
    }
}
function LoadTestNotes() {
    var projectsElem = document.querySelector('.js-notes-test');

    const items = [];
    items.length = 30;

    for (var i = 0; i < 30; i++) {

        const uniqueId = generateUniqueId();
        const dropdownContentElem = document.createElement('div');
        dropdownContentElem.className = 'dropdown-content';
        dropdownContentElem.innerHTML = `
                    <a href="#Edit">Edit</a>
                    <a href="#Remove">Remove</a>
        `;
        dropdownContentElem.id = uniqueId;


        const navElem = document.createElement('li');
        navElem.className = 'note nav-link dropdown';
        navElem.innerHTML = `
                        <a>
                            <div class="note-content">
                                <div class="nowrap-ellipsis">Note ${dropdownContentElem.id}</div>
                                <div class="nowrap-ellipsis note-description">Lorem Ipsum is simply dummy text of the printing and typesetting</div>
                                <div class="tools-row">
                                    <div class="d-flex min-width-0">
                                        <span class="material-symbols-outlined md-20 black">
                                            date_range
                                        </span>
                                        <div class="nowrap-ellipsis">
                                            6 nov
                                        </div>
                                    </div>
                                    <div class="d-flex">
                                        <span class="material-symbols-outlined md-20 black">
                                            label
                                        </span>
                                        <div class="nowrap-ellipsis">
                                            Dev
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <span class="material-symbols-outlined md-24 black dropdown-btn-ellipsis">
                                more_horiz
                            </span>
                        </a>
        `;

        const dropdownBtnElem = navElem.getElementsByClassName('dropdown-btn-ellipsis')[0];
        dropdownBtnElem.id = uniqueId;

        dropdownBtnElem.addEventListener('click', (event) => {
            toggleDropdown(event);
        });

        navElem.appendChild(dropdownContentElem);
        projectsElem.appendChild(navElem);
    }
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
