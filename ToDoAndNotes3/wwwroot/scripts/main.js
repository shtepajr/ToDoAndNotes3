$(function () {
    checkWindowSize();
    $(window).on('resize', checkWindowSize);
    $('#burger-menu-toggle').on('click', function () {
        $('#sidebar').toggleClass('sidebar-hide');
        checkWindowSize();
    });

    partialsController.configureProjectPartials();

    // parent element (document) in this case allow to set handlers for selected elements (that will be added to html later)
    $(document).on('click', '.create-submit-btn', partialsController.submitHandler);
    $(document).on('click', '.edit-submit-btn', partialsController.submitHandler);
    $(document).on('click', '.delete-submit-btn', partialsController.submitHandler);
    $(document).on('click', '.dropdown-btn', function () {
        let dropdown = document.getElementById($(this).attr('data-target-id'));
        dropdown.classList.toggle('show');
        let dropdowns = document.getElementsByClassName('dropdown-content');
        for (let olddropdown of dropdowns) {
            if (olddropdown.id !== dropdown.id) {
                olddropdown.classList.remove('show'); // hide previous dropdown
            }
        }
    });
    $(document).on('click', '.modal-button', function () {
        let modal = document.getElementById($(this).attr('data-target-id'));
        modal.style.display = 'block';
    });
    // parent element (document)
    $(document).on('click', function (event) {
        if (!event.target.classList.contains('dropdown-btn')) {
            let dropdowns = document.getElementsByClassName('dropdown-content');
            for (let dropdown of dropdowns) {
                dropdown.classList.remove('show'); // hide dropdown if click outside
            }
        }
        if (event.target.classList.contains('modal') || event.target.classList.contains('close')) {
            let modals = document.getElementsByClassName('modal');
            for (let modal of modals) {
                modal.style.display = "none"; // do not show anything if click outside
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

const partialsController = {
    configureProjectPartials() {
        $('.modal-button').click(function () {
            let targetId = $(this).data('target-id');

            // GET: ADD PROJECT modal
            if (targetId.includes('add-project-modal')) {
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
        });

    },
    submitHandler() {
        switch (true) {
            case $(this).hasClass('create-submit-btn'):
                break;
            case $(this).hasClass('edit-submit-btn'):
                let targetId = $(this).data('target-id');
                var token = $('input[name="__RequestVerificationToken"]').val();
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
                                $(`#${modalId}`).find('.modal-content').html(result);
                            }
                        }
                    });
                }
                break;
            case $(this).hasClass('delete-submit-btn'):
                break;
            default:
        }    
    }
}