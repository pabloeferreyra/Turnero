var dt;
$(document).ready(function () {
    new DateTime(document.getElementById('DateTurn'), {
        format: 'DD/MM/YYYY',
        i18n: {
            previous: 'Anterior',
            next: 'Siguiente',
            months: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
            weekdays: ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sab']
        }, 
        disableDays: [0],
    });

    dt = $('#turns').dataTable({
            "processing": true,
            "serverSide": true,
            "bFilter": true,
            "bDestroy": true,
            "bJQueryUI": true,
            "responsive": true,
            "ordering": true,
            "orderMulti": true,
            "autoWidth": true,
            "order": [[7, 'asc']],
            "dom": '<"row"<"col-md-6 col-sm-12"B>><"row table-responsive"rt><"row"<"col-md-6 col-sm-12"i><"col-md-6 ms-auto"p>>',
            "language": {
                "lengthMenu": "Mostrando _MENU_ filas por página",
                "zeroRecords": "No se encontro registro",
                "info": "Mostrando página _PAGE_ de _PAGES_",
                "infoEmpty": "No se encontraron resultados",
                "infoFiltered": "(Filtrado de _MAX_ total )",
                "search": "Buscar:",
                "paginate": {
                    "first": "Primera",
                    "last": "Última",
                    "next": "Siguiente",
                    "previous": "Anterior"
                },
                "buttons": {
                    "pageLength": {
                        _: "Mostrando %d filas",
                        '-1': "Todas las filas"
                    },
                    "collection": "Exportar"
                }
            },
            "ajax": {
                "url": "/Turns/InitializeTurns",
                "type": "Post",
                "datatype": "json"
            },

            "columns": [
                { "data": "name", "name": "Name" },
                { "data": "dni", "name": "Dni" },
                { "data": "socialWork", "name": "Social Work" },
                { "data": "reason", "name": "Reason" },
                { "data": "medicName", "name": "Medic" },
                { "data": "medicId", "name": "MedicId" },
                { "data": "date", "name": "Date" },
                { "data": "time", "name": "Time" },
                { "data": null },
            ],
            "columnDefs": [
                {
                    "targets": [-1],
                    "data": null,
                    "render": function (data) {
                        if (data['accessed'] == false && data['isMedic'] == false) {
                            return '<a href="/Turns/Edit/' + data['id']+'" class="btn btn-secondary">Editar</a>' +
                                '<span id="confirmaccessedSpan_' + data['id'] +'" style="display:none">' +
                                '<button onclick="Delete(\'' + data['id'] +'\');" class="btn btn-danger">Si</button>' +
                                '<a href="#" class="btn btn-primary"onclick="ConfirmDelete(\'' + data['id'] +'\', false)">No</a>'+
                                '</span >' +
                                '<span id="accessedSpan_' + data['id'] +'">' +
                                    '<a href="#" class="btn btn-danger"onclick="ConfirmDelete(\'' + data['id'] +'\', true)">Eliminar</a>' +
                                    '</span>';
                        }
                        else if (data['accessed'] == false && data['isMedic'] == true) {
                            return '<span id="confirmaccessedSpan_' + data['id'] +'" style = "display:none">' +
                                '<button onclick = "Accessed(\'' + data['id'] +'\')" class="btn btn-danger"> Si</button>' +
                                '<a href="#" class="btn btn-primary" onclick="ConfirmAccess(\'' + data['id'] +'\', false)">No</a>' +
                                    '</span>' +
                                '<span id="accessedSpan_'+data['id']+'">' +
                                '<a href="#" class="btn btn-primary" onclick="ConfirmAccess(\''+data['id']+'\', true)">Ingreso</a>'+
                                    '</span> ';
                        }

                        return '';
                    }
                },
                {
                    "target": 5,
                    "visible": false,
                    "searchable": true
                },
                { responsivePriority: -1, targets: 0 }
            ],
            "createdRow": function (row, data, dataIndex) {
                if (data['accessed'] == true) {
                    $(row).addClass('Red');
                }
            },
            lengthMenu: [
                [10, 25, 50, -1],
                ['10 filas', '25 filas', '50 filas', 'Todo']
            ],
            buttons: [
                {
                    extend: 'collection',
                    className: 'btn btn-primary',
                    buttons: [
                        {
                            extend: 'excelHtml5',
                            title: "Turnos",

                            exportOptions: {
                                columns: [0, 1, 2, 3, 4, 6, 7]
                            }
                        },
                        {
                            extend: 'pdfHtml5',
                            download: 'open',
                            orientation: 'landscape',
                            pageSize: 'A4',
                            title: "Turnos",
                            exportOptions: {
                                columns: [0, 1, 2, 3, 4, 6, 7]
                            }
                        },
                    ]
                },
                'pageLength',
                {
                    text: 'Recarga',
                    className: 'btn btn-primary',
                    action: function (dt) {
                        reset();
                    }
                }
            ]
        });

    oTable = $('#turns').DataTable();
    $('#btnSearch').click(function () {
        oTable.columns(5).search($('#Medics').val().trim());
        oTable.columns(6).search($('#DateTurn').val().trim());
        oTable.draw();
    });
});

function reset() {
    dt.api().ajax.reload();
}

var currentDate = "";
var time = "";
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
    //idleTimer();
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

function Accessed(id) {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    $.ajax({
        type: "POST",
        url: "/Turns/Accessed",
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
            }
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: 'Paciente accedio correctamente',
                showConfirmButton: false,
                timer: 1200
            });
            return reset();
        },
        error: function (req, status, error) {
        }
    });
}

function Delete(id) {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    $.ajax({
        type: "DELETE",
        url: "/Turns/Delete",
        data: {
            __RequestVerificationToken: token,
            id: id
        },
        success: function (result) {
            Swal.fire({
                position: 'top-end',
                icon: 'info',
                title: 'Turno eliminado Correctamente.',
                showConfirmButton: false,
                timer: 1200
            });
            return reset();
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

//function Edit(id) {
//    $.ajax({
//        type: "GET",
//        url: "/Turns/Edit",
//        data: {
//            id: id
//        }
//        window.location
//    })
//}

function AddNumber() {
    $('#pNumber').val(parseInt($('#pNumber').val()) + 1);
}

function RemNumber() {
    $('#pNumber').val(parseInt($('#pNumber').val()) - 1);
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
});

//-----------------------------------------------------  turnos  -----------------------------------------------------//