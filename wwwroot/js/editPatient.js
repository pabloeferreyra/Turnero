$(document).ready(function () {
    $("#Name").blur(function () {
        if ($("#Name").val() == '') {
            $("#NameValidation").text('Requerido');
            Swal.fire({
                position: 'top-end',
                icon: 'info',
                title: 'Por favor ingrese nombre.',
                showConfirmButton: false,
                timer: 600
            });
            $("#btnGuardarCambios").prop('disabled', true);
        }
        else {
            $("#NameValidation").text('');
            $("#btnGuardarCambios").prop('disabled', false);
        }
    });
    
    $("#phone").blur(function () {
        if ($("#phone").val().length < 6) {
            $("#phoneValidation").text('El teléfono debe tener por lo menos 6 (seis) caracteres.');
            Swal.fire({
                position: 'top-end',
                icon: 'info',
                title: 'El teléfono debe tener por lo menos 6 (seis) caracteres.',
                showConfirmButton: false,
                timer: 600
            });
            $("#btnGuardarCambios").prop('disabled', true);
        }
        else {
            $("#phoneValidation").text('');
            $("#btnGuardarCambios").prop('disabled', false);
        }
    });
});

$("#btnEditarPaciente").on('click', function (event) {
    event.preventDefault();
    Edit();
});

function Edit() {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    let formData = $("#EditForm").serialize();
    $.ajax({
        type: "PUT",
        url: "/Patients/Edit",
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        header: token,
        data: formData,
        success: function () {
            $("#Edit").modal('toggle');
            reset();
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: 'Paciente editado correctamente.',
                showConfirmButton: false,
                timer: 600
            })
        }
    });
}