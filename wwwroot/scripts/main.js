$(function () {
    var observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.addedNodes.length > 0) {
                $('#create-task-due-date').trigger('change');
            }
        });
    });
    var observerConfig = {
        childList: true,
        subtree: true
    };
    observer.observe(document.body, observerConfig);

    checkWindowSize();
    $(window).on('resize', checkWindowSize);
    $('#burger-menu-toggle').on('click', function () {
        $('#sidebar').toggleClass('sidebar-hide'); // css class toggle
        checkWindowSize();
    });
    $(document).on('click', '.js-modal-btn', function () {
        let targetModalId = $(this).data('target-modal-id');
        let parentForm = $(this).closest('form');
        let formMethod = parentForm.attr('method');
        let formAction = parentForm.attr('action');

        // GET: create/edit project partial
        // GET: create/edit/delete label partial
        // GET: create task partial
        $.ajax({
            url: formAction,
            type: formMethod,
            success: function (result) {
                $(`#${targetModalId}`).html(result);
                $(`#${targetModalId}`).find('.js-picker-input').trigger('blur');
            }
        });
    });
    $(document).on('click', '.js-submit-btn', function () {
        let parentModal = $(this).closest('.js-modal');
        let parentForm = $(this).closest('form');
        let formMethod = parentForm.attr('method');
        let formAction = parentForm.attr('action');
        let formToken = parentForm.find('input[name="__RequestVerificationToken"]').val();
        let formData = parentForm.serialize();

        // POST: create/edit project partial
        // POST: create/edit/delete label partial
        // POST: create task partial
        $.ajax({
            url: formAction,
            type: formMethod,
            data: formData,
            headers: {
                RequestVerificationToken: formToken
            },
            success: function (result) {
                if (result.success) {
                    location.reload(true);
                }
                else {
                    parentModal.html(result);
                }
            }
        });
    });

    function checkWindowSize() {
        const aside = $('#sidebar');
        const main = $('#main');

        // hide main if sidebar is shown on small screen
        if (window.innerWidth <= 635 && !aside.hasClass('sidebar-hide')) {
            main.css('opacity', 0);
            setTimeout(() => {
                main.css('display', 'none');
            }, 200);
        }
        else {
            main.css('opacity', 1);
            setTimeout(() => {
                main.css('display', 'block');
            }, 200);
        }
    }
});