$(document).on('click', '.js-dropdown-btn', function () {
    let dropdown = document.getElementById($(this).attr('data-target-id'));
    dropdown.classList.toggle('show');
    let dropdowns = document.getElementsByClassName('js-dropdown-content');
    for (let olddropdown of dropdowns) {
        if (olddropdown.id !== dropdown.id) {
            olddropdown.classList.remove('show'); // hide previous dropdown
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