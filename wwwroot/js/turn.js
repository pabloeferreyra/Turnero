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
    idleTimer();
});

function idleTimer() {
    var t;
    window.onload = resetTimer;
    window.onmousemove = resetTimer; // catches mouse movements
    window.onmousedown = resetTimer; // catches mouse movements
    window.onclick = resetTimer;     // catches mouse clicks
    window.onscroll = resetTimer;    // catches scrolling
    window.onkeypress = resetTimer;  //catches keyboard actions

    function reload() {
        window.location = self.location.href;  //Reloads the current page
    }

    function resetTimer() {
        clearTimeout(t);
        t = setTimeout(reload, 50000);  // time is in milliseconds (1000 is 1 second)
    }
}


function ConfirmAccess(uniqueId, isAccessedClicked) {
    var accessedSpan = 'accessedSpan_' + uniqueId;
    var confirmaccessedSpan = 'confirmaccessedSpan_' + uniqueId;

    if (isAccessedClicked) {
        $('#' + accessedSpan).hide();
        $('#' + confirmaccessedSpan).show();
    } else {
        $('#' + accessedSpan).show();
        $('#' + confirmaccessedSpan).hide();
    }
}

function accessed(urlAction, id) {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    $.ajax({
        type: "POST",
        url: urlAction,
        data: {
            __RequestVerificationToken: token,
            id: id
        },
        success: function (result) {
            if (result.trim().length == 0) {
                toastr.success('no quedan turnos!', 'Todo listo!');
                $("#TurnsPartial").html(result);
            }
            toastr.success('Paciente accedio correctamente', 'Accedido');
            $("#TurnsPartial").html(result);
        },
        error: function (req, status, error) {
        }
    });
}

function Delete(urlAction, id) {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    $.ajax({
        type: "POST",
        url: urlAction,
        data: {
            __RequestVerificationToken: token,
            id: id
        },
        success: function (result) {
            $("#TurnsPartial").html(result);
            toastr.success('Turno eliminado Correctamente.', 'Correcto!');
        },
        error: function (req, status, error) {
        }
    });
}

function ConfirmDelete(uniqueId, isAccessedClicked) {
    var accessedSpan = 'accessedSpan_' + uniqueId;
    var confirmaccessedSpan = 'confirmaccessedSpan_' + uniqueId;

    if (isAccessedClicked) {
        $('#' + accessedSpan).hide();
        $('#' + confirmaccessedSpan).show();
    } else {
        $('#' + accessedSpan).show();
        $('#' + confirmaccessedSpan).hide();
    }
}

function Create(urlAction) {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    $.ajax({
        type: "POST",
        url: urlAction,
        dataType: "json",
        contentType: "application/json",
        data: {
            __RequestVerificationToken: token,
            Name: $("#clientName").val(),
            Dni: $("#dniCliente").val(),
            MedicId: $("#medicId :selected").val(),
            TimeId: $("#timeTurn :selected").val(),
            DateTurn: $("#dateTurn").val(),
            SocialWork: $("#socialWork").val(),
            Reason: $("#reason").val()
        },
        success: function () {
            toastr.success('Turno creado correctamente.', 'Correcto!').delay(800);
        },
    });
}

function AddNumber() {
    $('#pNumber').val(parseInt($('#pNumber').val()) + 1);
}

function RemNumber() {
    $('#pNumber').val(parseInt($('#pNumber').val()) - 1);
}

function SearchTurns(urlAction) {
    var date = $("#DateTurn").val();
    var medic = $("#Medics :selected").val();
    var pNum = $('#pNumber').val();
    $.ajax({
        type: "GET",
        url: urlAction,
        data: {
            dateTurn: date,
            medicId: medic,
            pageNumber: pNum
        },
        success: function (result) {
            if (result.trim().length == 0) {
                toastr.success('no quedan turnos!', 'Todo listo!');
                $("#TurnsPartial").html(result);
            }
            $("#TurnsPartial").html(result);
        },
        error: function (req, status, error) {
        }
    });
}

function SearchAllTurns(urlAction) {
    $.ajax({
        type: "GET",
        url: urlAction,
        data: {
            dateTurn: null,
            medicId: null,
            pageNumber: null
        },
        success: function (result) {
            if (result.trim().length == 0) {
                toastr.success('no quedan turnos!', 'Todo listo!');
                $("#TurnsPartial").html(result);
            }
            $("#TurnsPartial").html(result);
        },
        error: function (req, status, error) {
        }
    });
}

//-----------------------------------------------------  turnos  -----------------------------------------------------//
$("#clientName").blur(function () {
    if ($("#clientName").val() == '') {
        $("#clientValidation").text('Por favor ingrese nombre de cliente.');
        toastr.error('Por favor ingrese nombre de cliente.', 'Error');
        $("#btnCrearTurno").prop('disabled', true);
    }
    else {
        $("#btnCrearTurno").prop('disabled', false);
    }
});

$("#dniCliente").blur(function () {
    if ($("#dniCliente").val() == '') {
        $("#dniValidation").text('Por favor ingrese DNI de cliente.');
        toastr.error('Por favor ingrese DNI de cliente.', 'Error');
        $("#btnCrearTurno").prop('disabled', true);
    }
    else {
        if ($("#dniCliente").val().length < 6) {
            $("#dniValidation").text('El DNI debe tener por lo menos 6 (seis) caracteres.');
            toastr.error('El DNI debe tener por lo menos 6 (seis) caracteres.', 'Error');
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
        toastr.error('la fecha no puede ser anterior a la actual.', 'Error');
        $("#btnCrearTurno").prop('disabled', true);
    }
    else {
        $("#btnCrearTurno").prop('disabled', false);
    }
});
$("#timeTurn").blur(function () {
    if ($("#timeTurn :selected").text() <= time && ($("#dateTurn").val() <= currentDate)) {
        $("#timeValidation").text('la hora no puede ser anterior a la actual.');
        $("#btnCrearTurno").prop('disabled', true);
    }
    else {
        $.ajax({
            type: "POST",
            url: "/Turns/CheckTurn",
            data: {
                medicId: $("#medicId :selected").val(),
                date: $("#dateTurn").val(),
                timeTurn: $("#timeTurn :selected").val()
            },
            complete: function (msj) {
                value = msj.responseText;
                if (value == 'false') {
                    $("#timeValidation").text('');
                    $("#btnCrearTurno").prop('disabled', false);
                }
                else {
                    $("#timeValidation").text('el turno ya existe, seleccione otro.');
                    toastr.error('el turno ya existe, seleccione otro.', 'Error');
                    $("#btnCrearTurno").prop('disabled', true);
                }
            }
        });

    }
});
//-----------------------------------------------------  turnos  -----------------------------------------------------//