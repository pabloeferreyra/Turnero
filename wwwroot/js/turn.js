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
        "responsive": false,
        "scrollX": true,
        "scrollCollapse": true,
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
            { "data": "name", "name": "Name", "width": '14%' },
            { "data": "dni", "name": "Dni", "width": '14%' },
            { "data": "socialWork", "name": "Social Work", "width": '14%' },
            { "data": "reason", "name": "Reason", "width": '14%' },
            { "data": "medicName", "name": "Medic", "width": '14%' },
            { "data": "medicId", "name": "MedicId", "width": '14%' },
            { "data": "date", "name": "Date", "width": '14%' },
            { "data": "time", "name": "Time", "width": '14%' },
            { "data": null, "width": '14%' },
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

function Delete(id) {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    $.ajax({
        type: "POST",
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



$("#btnEditarTurno").on('click', function (event) {
    event.preventDefault();
    EditTurn();
});

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



function EditTurn() {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    let formData = $("#EditForm").serialize();
    console.log(formData);
    $.ajax({
        type: "POST",
        url: "/Turns/Edit",
        data: {
            __RequestVerificationToken: token,
            formData
        },
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

