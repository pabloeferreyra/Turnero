$(function () {
    var tdate = new Date();
    var dd = tdate.getDate(); //yields day
    var MM = tdate.getMonth(); //yields month
    var yyyy = tdate.getFullYear(); //yields year
    if (dd < 10) {
        dd = '0' + dd;
    }
    if (MM < 9) {
        MM = '0' + (MM + 1);
    }
    var currentDate = yyyy + "-" + MM + "-" + dd;

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
//-----------------------------------------------------  turnos  -----------------------------------------------------//

});