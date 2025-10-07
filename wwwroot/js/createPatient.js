$(document).on('blur', '#Name', function () {
    if ($("#Name").val() == '') {
        $("#clientValidation").text('Requerido');
        Swal.fire({
            position: 'top-end',
            icon: 'info',
            title: 'Por favor ingrese nombre de paciente.',
            showConfirmButton: false,
            timer: 600
        });
        $("#btnCrearPaciente").prop('disabled', true);
    }
    else {
        $("#clientValidation").text('');
        $("#btnCrearPaciente").prop('disabled', false);
    }
});

$(document).on('change', '#Dni', function () {
    var $dni = $(this);
    var val = ($dni.val() || '').trim();
    console.log('DNI debug -> value:', val, 'length:', val.length);
    if (val.length < 6) {
        $("#dniValidation").text('El DNI debe tener por lo menos 6 (seis) caracteres.');
        Swal.fire({ position: 'top-end', icon: 'info', title: 'El DNI debe tener por lo menos 6 (seis) caracteres.', showConfirmButton: false, timer: 600 });
        $("#btnCrearPaciente").prop('disabled', true);
    } else {
        $("#clientValidation").text('');
        $("#dniValidation").text('');
        $("#btnCrearPaciente").prop('disabled', false);
    }
});

$(document).on('blur', '#Phone', function () {
    var $phone = $(this);
    var val = ($phone.val() || '').trim();
    console.log('Phone debug -> value:', val, 'length:', val.length);
    if (val.length < 6) {
        $("#phoneValidation").text('El teléfono debe tener por lo menos 6 (seis) caracteres.');
        Swal.fire({ position: 'top-end', icon: 'info', title: 'El teléfono debe tener por lo menos 6 (seis) caracteres.', showConfirmButton: false, timer: 600 });
        $("#btnCrearPaciente").prop('disabled', true);
    } else {
        $("#phoneValidation").text('');
        $("#btnCrearPaciente").prop('disabled', false);
    }
});

$(document).on('click', '#btnCrearPaciente', function (e) {
    e.preventDefault();
    Create();
});

function Create() {
    var form = $('__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    let formData = $("#CreateForm").serialize();
    $.ajax({
        type: "POST",
        url: "/Patients/Create",
        data: formData,
        headers: { "RequestVerificationToken": token },
        contentType: "application/x-www-form-urlencoded; charset=UTF-8",
        success: function (response) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: 'Paciente creado con éxito.',
                showConfirmButton: false,
                timer: 600
            }).then(() => {
                $('#Create').modal('toggle'); 
            });
        },
        error: function (xhr, status, error) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Ocurrió un error al procesar la solicitud.'
            });
        }
    });
}

