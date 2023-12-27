document.addEventListener('DOMContentLoaded', function () {
    toggleSidebar();
    configureModals();
    configureDropdown();

    window.addEventListener('load', checkWindowSize);
    window.addEventListener('resize', checkWindowSize);
});

function toggleSidebar() {
    var burgerMenuToggle = document.getElementById('burger-menu-toggle');
    var sidebar = document.getElementById('sidebar');

    burgerMenuToggle.addEventListener('click', function () {
        sidebar.classList.toggle('sidebar-hide');
        checkWindowSize();
    });
}
function checkWindowSize() {
    const aside = document.getElementById('sidebar');
    const main = document.getElementById('main');

    // Hide main if (is on "mobile"" screen) + (sidebar is shown)
    if (window.innerWidth <= 635 && !aside.classList.contains('sidebar-hide')) {
        main.style.opacity = 0;
        setTimeout(() => {
            main.style.display = 'none';
        }, 200);       
    }
    else {
        main.style.opacity = 1;
        setTimeout(() => {
            main.style.display = 'block';
        }, 200);  
    }
}
function configureDropdown() {
    let btnElems = document.getElementsByClassName('dropdown-btn');
    let dropdownElems = document.getElementsByClassName('dropdown-content');

    for (let btn of btnElems) {
        btn.addEventListener('click', function () {
            let dropdown = document.getElementById(btn.getAttribute('data-target-id'));
            dropdown.classList.toggle('show');
            // Do not show previous
            for (let olddropdown of dropdownElems) {
                if (olddropdown.id !== dropdown.id) {
                    olddropdown.classList.remove('show');
                }
            }
        });
    }
    // Do not show anything if click outside
    window.addEventListener('click', function (event) {
        if (!event.target.classList.contains('dropdown-btn')) {
            for (let dropdown of dropdownElems) {
                if (dropdown.classList.contains('show')) {
                    dropdown.classList.remove('show');
                }
            }
        }
    })
}
function configureModals() {
    let btnElems = document.getElementsByClassName('modal-button');

    for (let i = 0; i < btnElems.length; i++) {
        let btn = btnElems[i];
        btn.addEventListener('click', function() {
            let modal = document.getElementById(btn.getAttribute('data-target-id'));
            modal.style.display = 'block';
        });
    }

    let modalElems = document.getElementsByClassName('modal');
    // Do not show anything if click outside
    window.addEventListener('click', function (event) {
        if (event.target.classList.contains('modal') || event.target.classList.contains('close')) {
            for (let modal of modalElems) {
                modal.style.display = "none";
            }
        }
    });

    configureProjectPartials();
}

function configureProjectPartials() {
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
                    $('.submit-button').on('click', handleEditSubmit); // post handler link
                }
            });
        }
    });

    // POST: EDIT PROJECT handler
    function handleEditSubmit() {
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
    }  
}