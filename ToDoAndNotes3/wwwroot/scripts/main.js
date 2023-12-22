document.addEventListener('DOMContentLoaded', function () {
    toggleSidebar();
    configureModals();

    window.addEventListener('load', checkWindowSize);
    window.addEventListener('resize', checkWindowSize);
    window.addEventListener('click', toggleDropdown);
});

function toggleSidebar() {
    var burgerMenuToggle = document.getElementById('burger-menu-toggle');
    var sidebar = document.getElementById('sidebar');

    burgerMenuToggle.addEventListener('click', function () {
        sidebar.classList.toggle('sidebar-hide');
        checkWindowSize();
    });
}
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
  
    if (event.target.classList.contains('js-dropdown-btn-ellipsis')) {
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
    //event.stopPropagation();  // Stop the event from propagating up the DOM tree
}
function configureModals() {
    let btnElems = document.getElementsByClassName('modal-button');
    for (let i = 0; i < btnElems.length; i++) {
        let btn = btnElems[i];

        btn.addEventListener('click', function() {
            let modal = document.getElementById(btn.getAttribute('data-target-id'));
            modal.style.display = 'block';
        });
    }

    let modalElems = document.getElementsByClassName('modal');
    window.onclick = function (event) {
        if (event.target.classList.contains('modal') || event.target.classList.contains('close')) {
            for (let i in modalElems) {
                if (typeof modalElems[i].style !== 'undefined') {
                    modalElems[i].style.display = "none";
                }
            }
        }
    }
}

