﻿var currentDate = "";
var time = "";
var table = $('#Turns1').DataTable({
    paging: true,
    fixedHeader: true,
    scrollY: 300,
    scrollCollapse: true,
    "searching": true,
    language: {
        "lengthMenu": "Mostrando _MENU_ filas por pagina",
        "zeroRecords": "No se encontro registro",
        "info": "Mostrando pagina _PAGE_ de _PAGES_",
        "infoEmpty": "No se encontraron resultados",
        "infoFiltered": "(Filtrado de _MAX_ total )",
        "search": "Buscar:",
        "paginate": {
            "first": "Primera",
            "last": "Ultima",
            "next": "Siguiente",
            "previous": "Previo"
        },
        buttons: {
            pageLength: {
                _: "Mostrando %d filas",
                '-1': "Todas las filas"
            },
            "collection": "Exportar"
        }
    },
    responsive: {
        details: {
            type: 'inline'
        }
    },
    columnDefs: [{ responsivePriority: 1, targets: 0 }],
    order: [0, 'asc'],
    dom: '<"col-md-6"B><"col-md-6 ms-auto"f><"col-md-12"rt><"row"<"col-md-6"i><"col-md-6 ms-auto"p>>',
    lengthMenu: [
        [10, 25, 50, -1],
        ['10 filas', '25 filas', '50 filas', 'Todos']
    ],
    buttons: [
        {
            extend: 'collection',
            className: 'custom-html-collection',
            buttons: [
                {
                    extend: 'excelHtml5',
                    title: "Turnos",
                    exportOptions: {
                        columns: [0, 1, 2, 3, 4, 5, 6]
                    }
                },
                {
                    extend: 'pdfHtml5',
                    download: 'open',
                    orientation: 'landscape',
                    pageSize: 'A4',
                    title: "Turnos",
                    exportOptions: {
                        columns: [0, 1, 2, 3, 4, 5, 6]
                    }
                },
            ]
        },
        'spacer',
        'pageLength'

    ]
});
function initTable() {
    table
        .buttons()
        .container();
    return table;
}

$(document).ready(function () {
    initTable();
    new DateTime(document.getElementById('DateTurn'), {
        buttons: {
            today: true,
        }, 
        i18n: {
            today: "Hoy",
            previous: 'Anterior',
            next: 'Siguiente',
            months: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
            weekdays: ['Dom', 'Lun', 'Mar', 'Miér', 'Jue', 'Vier', 'Sáb']
        }
    });
});

$(window).on("load", function () {

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
    currentDate = yyyy + "-" + MM + "-" + dd;
    time = h + ":" + m;
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
                Swal.fire({
                    position: 'top-end',
                    icon: 'info',
                    title: 'no quedan turnos!',
                    showConfirmButton: false,
                    timer: 1200
                });
                $("#TurnsPartial").html(result);
            }
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: 'Paciente accedio correctamente',
                showConfirmButton: false,
                timer: 1200
            });
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
            Swal.fire({
                position: 'top-end',
                icon: 'info',
                title: 'Turno eliminado Correctamente.',
                showConfirmButton: false,
                timer: 1200
            });
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
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: 'Turno creado correctamente.',
                showConfirmButton: false,
                timer: 1200
            });
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
                Swal.fire({
                    position: 'top-end',
                    icon: 'success',
                    title: 'No quedan turnos!',
                    showConfirmButton: false,
                    timer: 1200
                });
                $("#TurnsPartial").html(result);
                initTable();
            }
            $("#TurnsPartial").html(result);
            initTable();
        },
        error: function (req, status, error) {
        }
    });
}

function ExportTurns(urlAction) {
    var date = $("#DateTurn").val();
    var medic = $("#Medics :selected").val();
    $.ajax({
        type: "POST",
        url: urlAction,
        data: {
            date: date,
            medicId: medic
        },
        success: function (result) {
            if (result.trim().length != 0) {
                Swal.fire({
                    position: 'top-end',
                    icon: 'success',
                    title: 'Exportado con exito!',
                    showConfirmButton: false,
                    timer: 1200
                });
            }
            
        },
        error: function (req, status, error) {
            Swal.fire({
                position: 'top-end',
                icon: 'error',
                title: error,
                showConfirmButton: false,
                timer: 1200
            });
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
                Swal.fire({
                    position: 'top-end',
                    icon: 'info',
                    title: 'No quedan turnos!',
                    showConfirmButton: false,
                    timer: 1200
                });
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
        $("#clientValidation").text('Requerido');
        Swal.fire({
            position: 'top-end',
            icon: 'info',
            title: 'Por favor ingrese nombre de cliente.',
            showConfirmButton: false,
            timer: 1200
        });
        $("#btnCrearTurno").prop('disabled', true);
    }
    else {
        $("#btnCrearTurno").prop('disabled', false);
    }
});

$("#dniCliente").blur(function () {
    if ($("#dniCliente").val() == '') {
        $("#clientValidation").text('Requerido');
        Swal.fire({
            position: 'top-end',
            icon: 'info',
            title: 'Por favor ingrese DNI de cliente.',
            showConfirmButton: false,
            timer: 1200
        });
        $("#btnCrearTurno").prop('disabled', true);
    }
    else {
        if ($("#dniCliente").val().length < 6) {
            $("#dniValidation").text('El DNI debe tener por lo menos 6 (seis) caracteres.');
            Swal.fire({
                position: 'top-end',
                icon: 'info',
                title: 'El DNI debe tener por lo menos 6 (seis) caracteres.',
                showConfirmButton: false,
                timer: 1200
            });
            $("#btnCrearTurno").prop('disabled', true);
        }
        else {
            $("#btnCrearTurno").prop('disabled', false);
        }
    }
});

$("#dateTurn").blur(function () {
    if ($("#dateTurn").val() < currentDate) {
        $("#dateValidation").text('Requerido.');
        Swal.fire({
            position: 'top-end',
            icon: 'info',
            title: 'La fecha no puede ser anterior.',
            showConfirmButton: false,
            timer: 1200
        });
        $("#btnCrearTurno").prop('disabled', true);
    }
    else {
        $("#btnCrearTurno").prop('disabled', false);
    }
});
$("#timeTurn").blur(function () {
    if ($("#timeTurn :selected").text() <= time && ($("#dateTurn").val() <= currentDate)) {
        $("#timeValidation").text('Requerido.');
        Swal.fire({
            position: 'top-end',
            icon: 'info',
            title: 'La hora no puede ser anterior.',
            showConfirmButton: false,
            timer: 1200
        });
        $("#btnCrearTurno").prop('disabled', true);
    }
    //else {
    //    $.ajax({
    //        type: "POST",
    //        url: "/Turns/CheckTurn",
    //        data: {
    //            medicId: $("#medicId :selected").val(),
    //            date: $("#dateTurn").val(),
    //            timeTurn: $("#timeTurn :selected").val()
    //        },
    //        complete: function (msj) {
    //            value = msj.responseText;
    //            if (value == 'false') {
    //                $("#timeValidation").text('');
    //                $("#btnCrearTurno").prop('disabled', false);
    //            }
    //            else {
    //                $("#timeValidation").text('El turno ya fue tomado, seleccione otro.');
    //                Swal.fire({
    //                    position: 'top-end',
    //                    icon: 'info',
    //                    title: 'El turno ya fue tomado',
    //                    showConfirmButton: false,
    //                    timer: 1200
    //                });
    //                $("#btnCrearTurno").prop('disabled', true);
    //            }
    //        }
});

//-----------------------------------------------------  turnos  -----------------------------------------------------//