var currentDate = "";
var time = "";

$(document).ready(function () {

    function getYesterdayDate() {
        return new Date(new Date().getTime() - 24 * 60 * 60 * 1000);
    }

    new DateTime(document.getElementById('DateTurnEdit'), {
        format: 'DD/MM/YYYY',
        i18n: {
            previous: 'Anterior',
            next: 'Siguiente',
            months: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
            weekdays: ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sab']
        },
        disableDays: [0],
        minDate: getYesterdayDate()
    });

    var tdate = new Date();
    var dd = tdate.getDate(); //yields day
    var MM = tdate.getMonth() + 1; //yields month
    var yyyy = tdate.getFullYear(); //yields year
    var h = tdate.getHours();
    var m = tdate.getMinutes();
    if (h < 10) {
        h = '0' + h;
    }

    if (m < 10) {
        m = '0' + m;
    }

    if (dd < 10) {
        dd = '0' + dd;
    }

    if (MM < 10) {
        MM = '0' + MM;
    }
    currentDate = dd + "/" + MM + "/" + yyyy;
    time = h + ":" + m;
});
//-----------------------------------------------------  turnos  -----------------------------------------------------//
$("#clientName").blur(function () {
    if ($("#clientName").val() == '') {
        $("#clientValidation").text('Requerido');
        Swal.fire({
            position: 'top-end',
            icon: 'info',
            title: 'Por favor ingrese nombre de cliente.',
            showConfirmButton: false,
            timer: 600
        });
        $("#btnCrearTurno").prop('disabled', true);
    }
    else {
        $("#clientValidation").text('');
        $("#btnCrearTurno").prop('disabled', false);
    }
});

$("#dniCliente").blur(function () {
    if ($("#dniCliente").val().length < 6) {
        $("#dniValidation").text('El DNI debe tener por lo menos 6 (seis) caracteres.');
        Swal.fire({
            position: 'top-end',
            icon: 'info',
            title: 'El DNI debe tener por lo menos 6 (seis) caracteres.',
            showConfirmButton: false,
            timer: 600
        });
        $("#btnCrearTurno").prop('disabled', true);
    }
    else {
        $("#clientValidation").text('');
        $("#dniValidation").text('');
        $("#btnCrearTurno").prop('disabled', false);
    }
});

$("#timeTurn").blur(function () {
    if ($("#timeTurn :selected").text() <= time && ($("#DateTurnEdit").val() == currentDate)) {
        $("#timeValidation").text('Requerido.');
        Swal.fire({
            position: 'top-end',
            icon: 'info',
            title: 'La hora no puede ser anterior.',
            showConfirmButton: false,
            timer: 600
        });
        $("#btnCrearTurno").prop('disabled', true);
    }
    else {
        $("#timeValidation").text('');
    }
});
//-----------------------------------------------------  turnos  -----------------------------------------------------//


$("#btnEditarTurno").on('click', function (event) {
    event.preventDefault();
    EditTurn();
});


function EditTurn() {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    let formData = $("#EditForm").serialize();
    $.ajax({
        type: "PUT",
        url: "/Turns/Edit",
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        header: token,
        data: formData,
        success: function () {
            $("#Edit").modal('toggle');
            reset();
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: 'Turno editado correctamente.',
                showConfirmButton: false,
                timer: 600
            });
        },
    });
}