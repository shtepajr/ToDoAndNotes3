$(function () {
    checkWindowSize();
    $(window).on('resize', checkWindowSize);
    $('#burger-menu-toggle').on('click', function () {
        $('#sidebar').toggleClass('sidebar-hide'); // css class toggle
        checkWindowSize();
    });

    partialsController.configureProjectPartials();

    // parent element (document) in this case allow to set handlers for selected elements (that will be added to html later)
    $(document).on('click', '.js-create-submit-btn', partialsController.submitHandler);
    $(document).on('click', '.js-edit-submit-btn', partialsController.submitHandler);
    $(document).on('click', '.js-delete-submit-btn', partialsController.submitHandler);

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

const partialsController = {
    configureProjectPartials() {
        $('.js-modal-btn').click(function () {
            let targetId = $(this).data('target-id');

            // GET: CREATE PROJECT modal
            if (targetId.includes('create-project-modal')) {
                $.ajax({
                    url: '/Projects/CreatePartial',
                    type: 'GET',
                    success: function (result) {
                        $(`#${targetId}`).html(result);
                    }
                });
            }
            // GET: EDIT PROJECT modal
            if (targetId.includes('edit-project-modal')) {
                let projectId = $(this).data("target-id").replace('edit-project-modal-', '');
                $.ajax({
                    url: '/Projects/EditPartial',
                    type: 'GET',
                    data: { id: projectId },
                    success: function (result) {
                        $(`#${targetId}`).html(result);
                    }
                });
            }
            // GET: CREATE LABEL modal
            if (targetId.includes('create-label-modal')) {
                $.ajax({
                    url: '/Labels/CreatePartial',
                    type: 'GET',
                    success: function (result) {
                        $(`#${targetId}`).html(result);
                    }
                });
            }
            // GET: EDIT LABEL modal
            if (targetId.includes('edit-label-modal')) {
                let labelId = $(this).data("target-id").replace('edit-label-modal-', '');
                $.ajax({
                    url: '/Labels/EditPartial',
                    type: 'GET',
                    data: { id: labelId },
                    success: function (result) {
                        $(`#${targetId}`).html(result);
                    }
                });
            }
            // GET: DELETE LABEL modal
            if (targetId.includes('delete-label-modal')) {
                let labelId = $(this).data("target-id").replace('delete-label-modal-', '');
                $.ajax({
                    url: '/Labels/DeletePartial',
                    type: 'GET',
                    data: { id: labelId },
                    success: function (result) {
                        $(`#${targetId}`).html(result);
                    }
                });
            }
            // GET: CREATE TASK modal
            if (targetId.includes('create-task-modal')) {
                $.ajax({
                    url: '/Tasks/CreatePartial',
                    type: 'GET',
                    success: function (result) {
                        $(`#${targetId}`).html(result);
                    }
                });
            }
        });
    },
    submitHandler() {
        let targetId = $(this).data('target-id');
        let token = $('input[name="__RequestVerificationToken"]').val();

        switch (true) {
            case $(this).hasClass('js-create-submit-btn'):           
                if (targetId.includes('create-project-form')) {
                    let formData = $(`#${targetId}`).serialize();
                    let modalId = $(this).data('target-id').replace('create-project-form', 'create-project-modal');
                    $.ajax({
                        url: '/Projects/CreatePartial',
                        type: 'POST',
                        data: formData,
                        headers: {
                            RequestVerificationToken: token
                        },
                        success: function (result) {
                            if (result.success) {
                                location.reload(true);
                            }
                            else {
                                $(`#${modalId}`).html(result);
                            }
                        }
                    });
                }
                if (targetId.includes('create-label-form')) {
                    let formData = $(`#${targetId}`).serialize();
                    let modalId = $(this).data('target-id').replace('create-label-form', 'create-label-modal');
                    $.ajax({
                        url: '/Labels/CreatePartial',
                        type: 'POST',
                        data: formData,
                        headers: {
                            RequestVerificationToken: token
                        },
                        success: function (result) {
                            if (result.success) {
                                location.reload(true);
                            }
                            else {
                                $(`#${modalId}`).html(result);
                            }
                        }
                    });
                }
                if (targetId.includes('create-task-form')) {
                    let formData = $(`#${targetId}`).serialize();
                    let modalId = $(this).data('target-id').replace('create-task-form', 'create-task-modal');
                    $.ajax({
                        url: '/Tasks/CreatePartial',
                        type: 'POST',
                        data: formData,
                        headers: {
                            RequestVerificationToken: token
                        },
                        success: function (result) {
                            if (result.success) {
                                location.reload(true);
                            }
                            else {
                                $(`#${modalId}`).html(result);
                            }
                        }
                    });
                }
                break;
            case $(this).hasClass('js-edit-submit-btn'):
                if (targetId.includes('edit-project-form')) {
                    let formData = $(`#${targetId}`).serialize();
                    let modalId = $(this).data('target-id').replace('edit-project-form-', 'edit-project-modal-');
                    $.ajax({
                        url: '/Projects/EditPartial',
                        type: 'POST',
                        data: formData,
                        headers: {
                            RequestVerificationToken: token
                        },
                        success: function (result) {
                            if (result.success) {
                                location.reload(true);
                            }
                            else {
                                $(`#${modalId}`).html(result);
                            }
                        }
                    });
                }
                if (targetId.includes('edit-label-form')) {
                    let formData = $(`#${targetId}`).serialize();
                    let modalId = $(this).data('target-id').replace('edit-label-form-', 'edit-label-modal-');
                    $.ajax({
                        url: '/Labels/EditPartial',
                        type: 'POST',
                        data: formData,
                        headers: {
                            RequestVerificationToken: token
                        },
                        success: function (result) {
                            if (result.success) {
                                location.reload(true);
                            }
                            else {
                                $(`#${modalId}`).html(result);
                            }
                        }
                    });
                }
                break;
            default:
        }    
    }
}