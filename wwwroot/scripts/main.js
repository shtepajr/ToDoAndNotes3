$(function () {
    checkWindowSize();
    $(window).on('resize', checkWindowSize);
    $('#burger-menu-toggle').on('click', function () {
        $('#sidebar').toggleClass('sidebar-hide'); // css class toggle
        checkWindowSize();
    });
    $(document).on('click', '.js-modal-btn', function (event) {
        let targetModalId = $(this).data('target-modal-id');
        let targetModal = $(`#${targetModalId}`);
        let parentForm = $(this).closest('form');
        let formMethod = parentForm.attr('method');
        let formAction = parentForm.attr('action');

        let formData = new FormData(parentForm[0]);
        // create URL
        let returnUrl = formData.get('returnUrl'); // => /Home/Main?projectId=1
        let url = new URL(formAction, window.location.origin);
        let params = new URLSearchParams(url.search);
        params.set('returnUrl', returnUrl);
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