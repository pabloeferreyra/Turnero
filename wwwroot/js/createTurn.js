var currentDate = "";
var time = "";

$(document).ready(function () {

    function getYesterdayDate() {
        return new Date(new Date().getTime() - 24 * 60 * 60 * 1000);
    }

    new DateTime(document.getElementById('DateTurnCreate'), {
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
$("#Name").blur(function () {
    if ($("#Name").val() == '') {
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

$("#Dni").blur(function () {
    if ($("#Dni").val().length < 6) {
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

$("#timeTurnCreate").blur(function () {
    if (($("#timeTurnCreate :selected").text() <= time && ($("#DateTurnCreate").val() == currentDate)) || ($("#timeTurnCreate :selected").val() == '')) {
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
        $("#btnCrearTurno").prop('disabled', false);
    }
});

$("#btnCrearTurno").on('click', function (event) {
    event.preventDefault();
    Create();
});

function Create() {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    let formData = $("#CreateForm").serialize();
    $.ajax({
        type: "POST",
        url: "/Turns/Create",
        header: token,
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: formData,
        success: function () {
            $("#Create").modal('toggle');
            reset();
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: 'Turno creado correctamente.',
                showConfirmButton: false,
                timer: 600
            });
        },
    });
}
    //-----------------------------------------------------  turnos  -----------------------------------------------------//
