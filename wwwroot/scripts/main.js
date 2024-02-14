$(function () {
    checkWindowSize();
    $(window).on('resize', checkWindowSize);
    $('#burger-menu-toggle').on('click', function () {
        $('#sidebar').toggleClass('sidebar-hide'); // css class toggle
        checkWindowSize();
    });

    $(document).on('click', '.js-pass-data-to', function () {
        let targetId = $(this).data('receiver-id');
        let id = $(this).data('id'); // element data
        let target = $(`#${targetId}`);

        if (target.is('form')) {
            setIdToAction(target, id);
            target.trigger('submit'); // if target is one form also submit it
        }
        else {
            let forms = target.find('form');
            forms.each(function () {
                setIdToAction($(this), id);
            });
        }

        function setIdToAction(target, id) {
            let currentAction = target.attr('action');

            if (currentAction.includes('id=')) {
                let updatedAction = currentAction.replace(/id=[^\/]+/, 'id=' + id);
                target.attr('action', updatedAction);
            } else {
                target.attr('action', currentAction + (currentAction.includes('?') ? '&' : '?') + 'id=' + id);
            }
        }
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

        // create form action URL
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

         //GET: create/edit/delete project partial
         //GET: create/edit/delete label partial
         //GET: create/edit/delete task partial
        $.ajax({
            url: formAction,
            type: formMethod,
            success: function (result) {
                targetModal.html(result);
                targetModal.find('.js-picker-input').trigger('blur');
                targetModal.css('display', 'block');
                targetModal.find('.js-textarea-auto').trigger('focus');
                //targetModal.find('.js-set-labels-virtual-select').trigger('click');
                targetModal.find('.js-set-projects-virtual-select').trigger('blur');
                targetModal.initInnerVirtualSelects = initInnerVirtualSelects;
                targetModal.initInnerVirtualSelects();

                // set openModal param
                let mainFullUrl = new URL(window.location.href);
                let mainParams = mainFullUrl.searchParams;

                mainParams.set('openModal', formAction);
                mainFullUrl.searchParams = mainParams;
                window.history.pushState({}, '', mainFullUrl.toString());
            }
        });
    });
    $(document).on('submit', '.js-post-partial-form', function (event) {
        event.preventDefault();

        let targetModalId = $(this).data('target-modal-id');
        let targetModal = $(`#${targetModalId}`);
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
                    targetModal.html(result);
                    targetModal.find('.js-picker-input').trigger('blur');
                }
            }
        });
    });
    $(document).on('submit', '.js-change-temp-data', function (event) {
        event.preventDefault();

        console.log('hello');

        let formAction = $(this).attr('action');
        let formData = $(this).serialize();
        let formToken = $(this).find('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: formAction,
            type: 'POST',
            data: formData,
            headers: {
                RequestVerificationToken: formToken
            },
            success: function (result) {
                if (result.success) {
                    window.location.href = result.redirectTo;
                    console.log('success');
                }
            },
            error: function (xhr, status, error) {
                console.log('error');
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

    $(document).on('click', '.js-task-is-done', function () {
        let isChecked = this.innerHTML.includes('radio_button_checked');

        if (isChecked) {
            this.innerHTML = 'radio_button_unchecked';
            $(this).closest('form')
            $(this).closest('form').find('input[name="Task.IsCompleted"]').val(false);
        } else {
            this.innerHTML = 'radio_button_checked';
            $(this).closest('form').find('input[name="Task.IsCompleted"]').val(true);
        }
    });

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

    $(document).on('click', function (event) {
        if (event.target.classList.contains('js-close')) {
            let url = new URL(window.location.href);
            url.searchParams.delete('openModal');
            window.history.pushState({}, '', url.toString());
        }
    });

    function initInnerVirtualSelects() {
        // projectSelect
        let projectSelect = $(this).find('.js-set-projects-virtual-select');
        if (projectSelect.length > 0) {
            let projectsOptions = projectSelect.data('target-select-list'); // [{..},{..},{..}]
            let projectsSelected = projectSelect.data('target-selected'); // number
            console.log(projectsOptions);
            console.log(projectsSelected);

            VirtualSelect.init({
                ele: projectSelect.get(0),
                options: projectsOptions.map(option => ({ label: option.Text, value: option.Value })),
                hideClearButton: true,
            });
            projectSelect.get(0).setValue(projectsSelected);

            // set value to its input
            projectSelect.on('change', function () {
                let selectedProject = $(this).val();
                $('input[name="Task.ProjectId"]').val(selectedProject);
                console.log($('input[name="Task.ProjectId"]').val());
            });
        }
      
        // labels
        let labelsSelect = $(this).find('.js-set-labels-virtual-select');
        if (labelsSelect.length > 0) {
            let labelsOptions = labelsSelect.data('target-select-list'); // ["12", "17"]
            let labelsSelected = labelsSelect.data('target-selected');
            console.log(labelsOptions);
            console.log(labelsSelected);

            VirtualSelect.init({
                ele: labelsSelect.get(0),
                options: labelsOptions.map(option => ({ label: option.Text, value: option.Value })),
                multiple: true,
                placeholder: "Labels",
                optionsSelectedText: "labels",
                optionSelectedText: "label",
                hideClearButton: true,
            });
            labelsSelect.get(0).setValue(labelsSelected);

            // set value to its input
            labelsSelect.on('change', function () {
                let selectedOptions = $(this).get(0).getSelectedOptions();
                let selectedValues = Array.from(selectedOptions).map(option => option.value);
                $('input[name="SelectedLabelsId"]').val(JSON.stringify(selectedValues));
                console.log($('input[name="SelectedLabelsId"]').val());
            });
        }
    }

    /* 
      Open modal:
      1. Main url: get openModal param
      2. Open modal url: path (form action)
      3. Open modal url: params (form inputs)
    */
    let urlParams = new URLSearchParams(window.location.search);
    let openModalUrl = urlParams.get('openModal');
    if (openModalUrl !== null) {
        let url = new URL(openModalUrl);
        let params = url.searchParams;
        let modalId;
        if (url.pathname.includes('Tasks')) {
            modalId = 'edit-task-modal';
        }
        else if (url.pathname.includes('Notes')) {
            modalId = 'edit-note-modal';
        }
        else if (url.pathname.includes('Projects')) {
            modalId = 'edit-project-modal';
        }
        else {
            return;
        }

        let form = $(
            `<form id="test" method="get" action=${url.pathname} class="js-get-partial-form" data-target-modal-id="${modalId}">
                <input type="hidden" name="id" value="${params.get('id')}">
                <input type="hidden" name="returnUrl" value="${params.get('returnUrl')}">
            </form>`
        );
        $('body').append(form);
        $('body').find('#test').trigger('submit');
    }
});