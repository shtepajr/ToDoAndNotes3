$(document).on('click', '.js-dropdown-btn', function (event) {
    // move dropdown-content to the body because parent container has overflow: auto (doesn't allow dropdown overlap parent)
    event.preventDefault(); // parent <a> will not work
    let dropdowns = document.getElementsByClassName('js-dropdown-content');
    Array.from(dropdowns).forEach(function (dropdown) {
        var oldParent = dropdown.parentNode;
        if (oldParent) {
            oldParent.removeChild(dropdown);
        }
        document.body.appendChild(dropdown);
    });

    // show
    let dropdown = document.getElementById($(this).attr('data-target-id'));
    dropdown.style.top = event.clientY + 10 + "px";
    dropdown.style.left = event.clientX + "px";
    dropdown.classList.toggle('show');

    if (event.clientY + dropdown.clientHeight > window.innerHeight) {
        dropdown.style.top = event.clientY - dropdown.clientHeight + "px";
    }
    if (event.clientX + dropdown.clientWidth > window.innerWidth) {
        dropdown.style.left = event.clientX - dropdown.clientWidth + "px";
    }

    // hide previous dropdown
    for (let olddropdown of dropdowns) {
        if (olddropdown.id !== dropdown.id) {
            olddropdown.classList.remove('show');
        }
    }
});
$(document).on('click', '.js-modal-btn', function () {
    let modal = document.getElementById($(this).attr('data-target-id'));
    modal.style.display = 'block';
});
// parent element (document)
$(document).on('click', function (event) {
    if (!event.target.classList.contains('js-dropdown-btn')) {
        let dropdowns = document.getElementsByClassName('js-dropdown-content');
        for (let dropdown of dropdowns) {
            dropdown.classList.remove('show'); // hide dropdown if click outside
        }
    }
    if (event.target.classList.contains('modal') || event.target.classList.contains('js-close')) {
        let modals = document.getElementsByClassName('modal');
        for (let modal of modals) {
            modal.style.display = "none"; // do not show anything if click outside
        }
    }
});