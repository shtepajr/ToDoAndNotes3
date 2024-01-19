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

    if (event.clientY + dropdown.clientHeight > window.visualViewport.height) {
        dropdown.style.top = event.clientY - dropdown.clientHeight + "px";
    }
    if (event.clientX + dropdown.clientWidth > window.visualViewport.width) {
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


function autoGrow(event) {   
    event.currentTarget.style.height = "5px"; //this.
    event.currentTarget.style.height = (event.currentTarget.scrollHeight) + "px";
}

function setAutoTextAreaHandlers() {
    let areas = document.getElementsByClassName('js-textarea-auto');
    Array.from(areas).forEach(function (area) {
        //area.setAttribute('rows', 1);
        area.addEventListener('focus', autoGrow);
        area.addEventListener('input', autoGrow);
        //area.addEventListener('load', autoGrow);
    });
}

// MutationObserver to detect changes in the DOM
var observer = new MutationObserver(function (mutations) {
    mutations.forEach(function (mutation) {
        // Check if nodes are added
        if (mutation.addedNodes.length > 0) {
            // Call setRows after a short delay to ensure the elements are rendered
            setTimeout(setAutoTextAreaHandlers, 100);
        }
    });
});
// Configure and start the MutationObserver
var observerConfig = {
    childList: true,  // Listen for changes to the child nodes of the target
    subtree: true     // Look at descendants as well
};
// Start observing the document body
observer.observe(document.body, observerConfig);

// Date, time js-picker
$(document).on('focus', '.js-picker', function (event) {
    if (event.currentTarget.getAttribute('name').includes('Time')) {
        event.currentTarget.type = "time";
    }
    else if (event.currentTarget.getAttribute('name').includes('Date')) {
        event.currentTarget.type = "date";
    }
    event.currentTarget.showPicker();
});
$(document).on('blur', '.js-picker', function (event) {
    if (!event.currentTarget.value) {
        event.currentTarget.type = "text";
    }
});