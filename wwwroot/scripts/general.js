$(function () {
    var observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.addedNodes.length > 0) {
                setAutoTextAreaHandlers();
                loadDateTimePicker();
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

    function loadDateTimePicker() {
        let dateTimePicker = $('.js-date-time-picker');

        let datePicker = dateTimePicker.find('.js-date-picker');
        let datePickerInput = datePicker.find('.js-picker-input');

        let timePicker = dateTimePicker.find('.js-time-picker');
        let timePickerInput = timePicker.find('.js-picker-input');

        dateTimePicker.trigger('blur');
        dateTimePicker.find('.js-date-picker').on('change', function () {
            dateTimePicker.trigger('blur');
        }); 
        dateTimePicker.on('blur', function () {
            // if date is not selected -> hide time picker
            if (datePickerInput.val() < 1) {
                timePickerInput.val('');
                timePicker.css('display', 'none');
                datePickerInput.attr('type', 'text');
                datePickerInput.prop('readonly', true);
            }
            else { // if date is selected -> show time picker
                timePicker.css('display', 'block');
                timePickerInput.attr('type', 'text');
                timePickerInput.prop('readonly', true);
            }
        });

        dateTimePicker.find('.js-date-picker').on('click', function () {
            if (datePickerInput.length > 0) {
                datePickerInput.prop('readonly', false);
                datePickerInput.attr('type', 'date');
                datePickerInput.get(0).showPicker();
            }
        });
        dateTimePicker.find('.js-time-picker').on('click', function () {
            if (timePickerInput.length > 0) {
                timePickerInput.prop('readonly', false);
                timePickerInput.attr('type', 'time');
                timePickerInput.get(0).showPicker();
            }
        });

        datePicker.find('.js-clear-input').on('click', function (event) {
            event.stopPropagation();
            datePickerInput.val('');
            dateTimePicker.trigger('blur');
        });
        timePicker.find('.js-clear-input').on('click', function (event) {
            event.stopPropagation();
            timePickerInput.val('');
            dateTimePicker.trigger('blur');
        });
        datePickerInput.on('blur', function () {
            dateTimePicker.trigger('blur');
        });
        timePickerInput.on('blur', function () {
            dateTimePicker.trigger('blur');
        });
    }
});