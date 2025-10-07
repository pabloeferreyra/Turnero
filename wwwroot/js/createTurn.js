var currentDate = "";
var time = "";

$(document).ready(function () {

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

    var isoDate = yyyy + "-" + MM + "-" + dd;
    currentDate = isoDate;
    time = h + ":" + m;

    var $dateInput = $('#DateTurnCreate');
    if ($dateInput.length) {
        $dateInput.val(isoDate);
        $dateInput.attr('min', isoDate);
    }
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
function validateDate($input, $btn) {
    var val = $input.val();
    if (!val) { $btn.prop('disabled', false); return true; }
    var d = new Date(val + 'T00:00:00');
    var today = new Date(); today.setHours(0, 0, 0, 0);
    var day = d.getDay(); // 0 Sun, 6 Sat
    if (d < today) {
        Swal.fire({ position: 'top-end', icon: 'info', title: 'No puede seleccionar fechas anteriores a hoy.', showConfirmButton: false, timer: 1200 });
        $("#btnCrearTurno").prop('disabled', true);
        return false;
    }
    if (day === 0 || day === 6) {
        Swal.fire({ position: 'top-end', icon: 'info', title: 'No puede seleccionar sábados o domingos.', showConfirmButton: false, timer: 1200 });
        $("#btnCrearTurno").prop('disabled', true);
        return false;
    }
    $("#btnCrearTurno").prop('disabled', false);
    return true;
}

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

(function () {
    function todayString() {
        var d = new Date();
        var yyyy = d.getFullYear();
        var mm = (d.getMonth() + 1).toString().padStart(2, '0');
        var dd = d.getDate().toString().padStart(2, '0');
        return yyyy + '-' + mm + '-' + dd;
    }

    var $input = $('#DateTurnCreate');
    var $btn = $('#btnCrearTurno');

    // asegurar atributo min
    if ($input.length) {
        $input.attr('min', todayString());
    }

    // vincular eventos para validar la fecha usando la función validateDate existente
    $(document).on('change input', '#DateTurnCreate', function () { validateDate($input, $btn); });

    // validación inicial si existe el input
    $(function () { if ($input.length) validateDate($input, $btn); });
})();
