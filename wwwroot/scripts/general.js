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
        event.preventDefault(); // parent <a> will not work  

        let dropContentId = $(this).data('dropdown-content-id');

        // get target id (e.g ProjectId)
        let targetId = $(this).data('target-id');
        let dropContent = $(`#${dropContentId}`);

        // set target id (e.g <form asp-controller="Projects" asp-action="Duplicate"> <input name="id" hidden />)
        let forms = dropContent.find('form');
        forms.each(function () {
            var formTargetInput = $(this).find('input[name="id"]');
            formTargetInput.val(targetId);
        });

        // show
        dropContent.css('top', event.clientY + 10 + "px");
        dropContent.css('left', event.clientX + "px");

        if (event.clientY + dropContent.outerHeight() > window.visualViewport.height) {
            dropContent.css('top', event.clientY - dropContent.outerHeight() + "px");
        }
        if (event.clientX + dropContent.outerWidth() > window.visualViewport.width) {
            dropContent.css('left', event.clientX - dropContent.outerWidth() + "px");
        }

        dropContent.toggleClass('show');

        $(document).find('.js-dropdown-content').not(dropContent).each(function () {
            $(this).removeClass('show');
        });
    });
    $(document).on('click', '.js-modal-btn', function () {
        let modal = document.getElementById($(this).attr('data-target-modal-id'));
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