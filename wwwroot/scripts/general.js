$(function () {
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
        event.stopPropagation(); // parent <a> task will not work  
        event.preventDefault(); // parent <a> project will not work  

        let dropContentId = $(this).data('dropdown-content-id');

        // get target id (e.g ProjectId)
        let targetId = $(this).data('target-id');
        let dropContent = $(`#${dropContentId}`);

        let forms = dropContent.find('form');
        forms.each(function () {
            let currentAction = $(this).attr('action');

            if (currentAction.includes('id=')) {
                let updatedAction = currentAction.replace(/id=[^\/]+/, 'id=' + targetId);
                $(this).attr('action', updatedAction);
            } else {
                $(this).attr('action', currentAction + (currentAction.includes('?') ? '&' : '?') + 'id=' + targetId);
            }
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

    // Text area auto grow
    function autoGrow(event) {
        var textarea = $(event.currentTarget);
        textarea.css('height', textarea[0].scrollHeight + 'px');
    }
    $(document).on('focus input', '.js-textarea-auto', autoGrow);
    // Date Time Picker
    $(document).on('click', '.js-date-picker', function () {
        let datePickerInput = $(this).find('.js-picker-input');

        if (datePickerInput.length > 0) {
            datePickerInput.prop('readonly', false);
            datePickerInput.attr('type', 'date');
            datePickerInput.get(0).showPicker();
        }
    });
    $(document).on('click', '.js-time-picker', function () {
        let timePickerInput = $(this).find('.js-picker-input');

        if (timePickerInput.length > 0) {
            timePickerInput.prop('readonly', false);
            timePickerInput.attr('type', 'time');
            timePickerInput.get(0).showPicker();
        }
    });
    $(document).on('click', '.js-clear-input',  function (event) {
        event.stopPropagation();
        let currentInput = $(this).siblings('.js-picker-input');
        currentInput.val('');
        currentInput.trigger('change');
        $(this).css('display', 'none');
    });
    $(document).on('blur change', '.js-picker-input', function () {
        let closestClearBtn = $(this).siblings('.js-clear-input');
        let currentDateTimePicker = $(this).closest('.js-date-time-picker');

        // if input empty
        if ($(this).val() < 1 || $(this).val() === '') {
            // check if it is date => then hide time picker + make input text to see placeholder
            // check if it is time => make input text to see placeholder
            if ($(this).attr('type') === 'date') {
                currentDateTimePicker.find('.js-time-picker').css('display', 'none');
                $(this).attr('type', 'text');
            }
            else if ($(this).attr('type') === 'time') {
                $(this).attr('type', 'text');
            }         
           
            $(this).prop('readonly', true);
            closestClearBtn.css('display', 'none');
        }
        else {
            closestClearBtn.css('display', 'block');
            currentDateTimePicker.find('.js-time-picker').css('display', 'block');;
        }
    });
});