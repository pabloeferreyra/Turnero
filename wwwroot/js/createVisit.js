let currentDate = "";

$(document).ready(function () {
    const tdate = new Date();
    const dd = String(tdate.getDate()).padStart(2, '0');
    const MM = String(tdate.getMonth() + 1).padStart(2, '0');
    const yyyy = tdate.getFullYear();
    currentDate = `${yyyy}-${MM}-${dd}`;

    const $dateInput = $('#VisitDate');
    if ($dateInput.length) {
        $dateInput.val(currentDate);
        $dateInput.attr('max', currentDate);
    }
});

function validateDate($input, $btn) {
    const val = $input.val();
    if (!val) {
        $btn.prop('disabled', false);
        return true;
    }

    const selected = new Date(`${val}T00:00:00`);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (selected > today) {
        Swal.fire({
            position: 'top-end',
            icon: 'info',
            title: 'No puede seleccionar fechas posteriores a hoy.',
            showConfirmButton: false,
            timer: 1200
        });
        $btn.prop('disabled', true);
        return false;
    }

    $btn.prop('disabled', false);
    return true;
}

function initCreateVisitForm() {
    const $form = $('#CreateVisitForm');
    const $btn = $('#btnCreateVisit');
    const $dateInput = $('#VisitDate');

    if (!$form.length || !$btn.length) return;

    validateDate($dateInput, $btn);

    $dateInput.on('input change', function () {
        validateDate($dateInput, $btn);
    });

    $btn.off('click').on('click', function (e) {
        e.preventDefault();

        if (!validateDate($dateInput, $btn)) return;

        const formData = $form.serializeArray().reduce((acc, field) => {
            acc[field.name] = field.value;
            return acc;
        }, {});

        const token = $form.find('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            type: 'POST',
            url: '/Visits/Create',
            contentType: 'application/json; charset=UTF-8',
            data: JSON.stringify(formData),
            headers: { 'RequestVerificationToken': token },
            success: function () {
                $('#CreateVisit').modal('hide');
                $form[0].reset();
                $('#VisitDate').val(currentDate);

                Swal.fire({
                    position: 'top-end',
                    icon: 'success',
                    title: 'Visita creada correctamente.',
                    showConfirmButton: false,
                    timer: 600
                });

                if (typeof window.reloadVisitsTable === 'function') {
                    window.reloadVisitsTable();
                } else {
                    console.warn('reloadVisitsTable no definido — verifica que visits.js esté cargado antes.');
                }
            },
            error: function (xhr) {
                let message = 'Error creando visita';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    message = xhr.responseJSON.message;
                }
                console.error('Error creando visita:', xhr);
                Swal.fire({
                    position: 'top-end',
                    icon: 'error',
                    title: message,
                    showConfirmButton: false,
                    timer: 1500
                });
            }
        });
    });
}

function CreateVisitView(patientId) {
    $.ajax({
        type: 'GET',
        url: '/Visits/Create',
        data: { id: patientId },
        success: function (data) {
            $('#CreateVisitFormContent').html(data);
            $('#CreateVisit').modal('show');
            initCreateVisitForm();
        },
        error: function (xhr, status, err) {
            console.error('Error cargando modal de visita', status, err, xhr?.responseText);
            Swal.fire({
                position: 'top-end',
                icon: 'error',
                title: 'Error al cargar el formulario de visita.',
                showConfirmButton: false,
                timer: 1200
            });
        }
    });
}
