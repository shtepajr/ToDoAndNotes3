$(function () {
    var observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.addedNodes.length > 0) {
                setTimeout(setAutoTextAreaHandlers, 100);
                setTimeout(triggerBlurable, 4);
            }
        });
    });
    var observerConfig = {
        childList: true,
        subtree: true
    };
    observer.observe(document.body, observerConfig);

    function setAutoTextAreaHandlers() {
        function autoGrow(event) {
            event.currentTarget.style.height = "5px";
            event.currentTarget.style.height = (event.currentTarget.scrollHeight) + "px";
        }
        let areas = document.getElementsByClassName('js-textarea-auto');
        Array.from(areas).forEach(function (area) {
            area.addEventListener('focus', autoGrow);
            area.addEventListener('input', autoGrow);
        });
    }
    function triggerBlurable() {
        $('.js-blurable').trigger('blur');
    }

    $(document).on('click', function (event) {
        if (!event.target.classList.contains('js-dropdown-btn')) {
            let dropdowns = document.getElementsByClassName('js-dropdown-content');
            for (let dropdown of dropdowns) {
                dropdown.classList.remove('show'); // hide dropdown if click outside
            }
        }
        if (/*event.target.classList.contains('modal') || */event.target.classList.contains('js-close')) {
            let modals = document.getElementsByClassName('modal');
            for (let modal of modals) {
                modal.style.display = "none"; // do not show anything if click outside
            }
        }
    });

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
    $(document).on('click', '.js-picker-preview', function (event) {
        var dateSubstring = 'DueDate';
        var datePickerElem = $(this).find('[name*="' + dateSubstring + '"]');

        var timeSubstring = 'DueTime';
        var timePickerElem = $(this).find('[name*="' + timeSubstring + '"]');

        // if picker exists -> show
        if (datePickerElem.length > 0) {
            datePickerElem.attr('type', 'date');
            datePickerElem.get(0).showPicker();
        }
        else if (timePickerElem.length > 0) {
            timePickerElem.attr('type', 'time');
            timePickerElem.get(0).showPicker();
        }
    });
    $(document).on('blur', '.js-picker-preview', function (event) {
        var dateSubstring = 'DueDate';
        var datePickerElem = $(this).find('[name*="' + dateSubstring + '"]')

        var timeSubstring = 'DueTime';
        var timePickerElem = $(this).find('[name*="' + timeSubstring + '"]')

        // if picker exists and empty -> to text type (to see placeholder)
        if (datePickerElem.length > 0) {
            if (datePickerElem.get(0).value.length < 1) {
                datePickerElem.attr('type', 'text');
            }
        }
        else if (timePickerElem.length > 0) {
            if (timePickerElem.get(0).value.length < 1) {
                timePickerElem.attr('type', 'text');
            }
        }
    });
});