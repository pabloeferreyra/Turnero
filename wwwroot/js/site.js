$(function () {
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
    var currentDate = yyyy + "-" + MM + "-" + dd;
    var time = h + ":" + m;

//-----------------------------------------------------  turnos  -----------------------------------------------------//
    $("#clientName").blur(function () {
        if ($("#clientName").val() == '') {
            $("#clientValidation").text('Por favor ingrese nombre de cliente.');
            $("#btnCrearTurno").prop('disabled', true);
        }
        else {
            $("#btnCrearTurno").prop('disabled', false);
        }
    });

    $("#dniCliente").blur(function () {
        if ($("#dniCliente").val() == '') {
            $("#dniValidation").text('Por favor ingrese DNI de cliente.');
            $("#btnCrearTurno").prop('disabled', true);
        }
        else {
            if ($("#dniCliente").val().length < 6) {
                $("#dniValidation").text('El DNI debe tener por lo menos 6 (seis) caracteres.');
                $("#btnCrearTurno").prop('disabled', true);
            }
            else {
                $("#btnCrearTurno").prop('disabled', false);
            }
        }
    });

    $("#dateTurn").blur(function () {
        if ($("#dateTurn").val() < currentDate) {
            $("#dateValidation").text('la fecha no puede ser anterior a la actual.');
            $("#btnCrearTurno").prop('disabled', true);
        }
        else {
            $("#btnCrearTurno").prop('disabled', false);
        }
    });
    $("#timeTurn").blur(function () {
        if ($("#timeTurn :selected").text() >= time && ($("#dateTurn").val() <= currentDate)){
            $("#timeValidation").text('la hora no puede ser anterior a la actual.');
            $("#btnCrearTurno").prop('disabled', true);
        }
        else {
            $("#timeValidation").text('');
            $("#btnCrearTurno").prop('disabled', false);
        }
    });
//-----------------------------------------------------  turnos  -----------------------------------------------------//

});
