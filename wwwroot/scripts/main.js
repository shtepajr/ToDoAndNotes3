$(function () {
    checkWindowSize();
    $(window).on('resize', checkWindowSize);
    $('#burger-menu-toggle').on('click', function () {
        $('#sidebar').toggleClass('sidebar-hide'); // css class toggle
        checkWindowSize();
    });
    $(document).on('click', '.js-submit-by-form-id', function () {
        let formId = $(this).data('form-id');
        let form = $(`#${formId}`);
        form.trigger('submit');
    });
    $(document).on('submit', '.js-get-partial-form', function (event) {
        event.preventDefault();

        let targetModalId = $(this).data('target-modal-id');
        let targetModal = $(`#${targetModalId}`);
        let formMethod = $(this).attr('method');
        let formAction = $(this).attr('action');

        let formData = new FormData($(this)[0]);
        let returnUrl = formData.get('returnUrl');
        let targetId = formData.get('id');
        // create URL
        let url = new URL(formAction, window.location.origin);
        let params = new URLSearchParams(url.search);
        if (!params.has('returnUrl')) {
            params.set('returnUrl', returnUrl);
        }
        if (!params.has('id')) {
            params.set('id', targetId);
        }
        url.search = params.toString();
        formAction = url.toString();

        // GET: create/edit/delete project partial
        // GET: create/edit/delete label partial
        // GET: create/edit/delete task partial
        $.ajax({
            url: formAction,
            type: formMethod,
            success: function (result) {
                targetModal.html(result);
                targetModal.find('.js-picker-input').trigger('blur');
                targetModal.css('display', 'block');
                targetModal.find('.js-textarea-auto').trigger('focus');
            }
        });
    });
    $(document).on('submit', '.js-post-partial-form', function (event) {
        event.preventDefault();

        let parentModal = $(this).closest('.js-modal');
        let form = $(this);
        let formMethod = form.attr('method');
        let formAction = form.attr('action');
        let formToken = form.find('input[name="__RequestVerificationToken"]').val();
        let formData = form.serialize();

        // POST: create/edit/delete project
        // POST: create/edit/delete label
        // POST: create/edit/delete task
        $.ajax({
            url: formAction,
            type: formMethod,
            data: formData,
            headers: {
                RequestVerificationToken: formToken
            },
            success: function (result) {
                if (result.success) {
                    window.location.href = result.redirectTo;
                }
                else {
                    parentModal.html(result);
                    parentModal.find('.js-picker-input').trigger('blur');
                }
            }
        });
    });
    function checkWindowSize() {
        let main = $('#main');

        let sidebarIsHidden = $('#sidebar').hasClass('sidebar-hide');

        // sidebar is shown on small screen => hide main
        if (!sidebarIsHidden && window.innerWidth < 635) {
            main.css('opacity', 0);
            setTimeout(() => {
                main.css('display', 'none');
            }, 200);
        } // sidebar is hidden or screen is bigger => show main
        else if (sidebarIsHidden || window.innerWidth > 635) {
            main.css('opacity', 1);
            setTimeout(() => {
                main.css('display', 'block');
            }, 200);
        }

        // if sidebar shown on bigger screen       
        if (!sidebarIsHidden && window.innerWidth > 635) {
            // smaller => 1 column
            if (window.innerWidth < 900) {
                $(document).find('.tasks').css('display', 'none');
                $(document).find('.notes').css('display', 'none');
                $(document).find('.tasks-with-notes').css('display', 'flex');

            } // bigger => 2 column   
            else {
                $(document).find('.tasks').css('display', 'flex');
                $(document).find('.notes').css('display', 'flex');
                $(document).find('.tasks-with-notes').css('display', 'none');
            }
        } //if sidebar is hidden
        else if (sidebarIsHidden) {
            // smaller => 1 column
            if (window.innerWidth < 590) {
                $(document).find('.tasks').css('display', 'none');
                $(document).find('.notes').css('display', 'none');
                $(document).find('.tasks-with-notes').css('display', 'flex');

            } // bigger => 2 column   
            else {
                $(document).find('.tasks').css('display', 'flex');
                $(document).find('.notes').css('display', 'flex');
                $(document).find('.tasks-with-notes').css('display', 'none');
            }
        }
    }

    $('.label-tools').each(function () {
        let labelsCount = $(this).children('.tools-label').length;
        var labelsToHide = $(this).children(".tools-label:gt(0)");
        labelsToHide.remove();  // if 1 + label

        if (labelsCount > 1) {
            $(this).append('+' + (labelsCount - 1));
        }
        else if (labelsCount === 1) {
            // do nothing
        }
        else {
            $(this).html(''); // empty if there is not labels
        }
    });
});