﻿var dt;
$(document).ready(function () {

    new DateTime(document.getElementById('DateTurn'), {
        format: 'DD/MM/YYYY',
        locale: 'es-ES',
        disableDays: [0],
    });


    dt = $('#turns').dataTable({
        "language": {
            url: '//cdn.datatables.net/plug-ins/1.13.4/i18n/es-ES.json',
            "buttons": {
                "collection": "Exportar"
            }
        },
        "processing": true,
        "serverSide": true,
        "bFilter": true,
        "bDestroy": true,
        "bJQueryUI": true,
        "responsive": false,
        "scrollX": false,
        "scrollCollapse": false,
        "ordering": true,
        "orderMulti": true,
        "autoWidth": true,
        "order": [[7, 'asc']],
        "dom": '<"row"<"col-md-6 col-sm-12"B>><"row table-responsive"rt><"row"<"col-md-6 col-sm-12"i><"col-md-6 ms-auto"p>>',
        "ajax": {
            "url": "/Turns/InitializeTurns",
            "type": "Post",
            "datatype": "json"
        },

        "columns": [
            { "data": "name", "name": "Name", "class": 'col-md-1' },
            { "data": "dni", "name": "Dni", "class": 'col-md-1' },
            { "data": "socialWork", "name": "Social Work", "class": 'col-md-1' },
            { "data": "reason", "name": "Reason", "class": 'col-md-1' },
            { "data": "medicName", "name": "Medic", "class": 'col-md-1' },
            { "data": "medicId", "name": "MedicId", "class": 'col-md-1' },
            { "data": "date", "name": "Date", "class": 'col-md-1' },
            { "data": "time", "name": "Time", "class": 'col-md-1' },
            { "data": null, "class": 'col-md-1' },
        ],
        "columnDefs": [
            {
                "targets": [-1],
                "data": null,
                "render": function (data) {
                    if (data['accessed'] == false && data['isMedic'] == false) {
                        return '<button onclick="Edit(\'' + data['id'] +'\');" class="btn btn-secondary">Editar</button>' +
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
            }
        ],
        "createdRow": function (row, data, dataIndex) {
            if (data['accessed'] == true) {
                $(row).addClass('Red');
            }
        },
        lengthMenu: [
            [-1, 25, 50],
            ['Todo','25 filas', '50 filas']
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

function Call(name, medicName) {
    $.ajax({
        type: "POST",
        url: "/Turns/Call",
        data: {
            Patient: name,
            MedicCaller: medicName
        },
        success: function (result) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: 'Llamando paciente',
                showConfirmButton: false,
                timer: 1200
            });
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

$("#createTurn").on('click', function (event) {
    CreateView();
    $("#Create").modal('toggle');
});

function CreateView() {
    $.ajax({
        type: "GET",
        url: "/Turns/Create",
        success: function (data) {
            $("#CreateFormContent").html(data);
        }
    })
}

function Edit(id) {
    $.ajax({
        type: "GET",
        url: "/Turns/Edit",
        data: {
            id: id
        },
        success: function (data) {
            $("#EditFormContent").html(data);
            $("#Edit").modal('toggle');
        }
    })
}

