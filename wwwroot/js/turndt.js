$(document).ready(function () {
    $('#turns').dataTable({
        "processing": true,
        "serverSide": true,
        "bFilter": true,
        "bDestroy": true,
        "bJQueryUI": true,
        "responsive": {
            "details": {
                "type": 'inline'
            }
        },
        "ordering": true,
        "orderMulti": false,
        "dom": '<"col-md-6"B><"col-md-12"rt><"row"<"col-md-6"i><"col-md-6 ms-auto"p>>',
        "language": {
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
        "columnDefs": [{
            "targets": "_all",
            "searchable": false
        }],
        "columns": [
            { "data": "name", "name": "Name", "autoWidth": true },
            { "data": "dni", "name": "Dni", "autoWidth": true },
            { "data": "socialWork", "name": "Social Work", "autoWidth": true },
            { "data": "reason", "name": "Reason", "autoWidth": true },
            { "data": "medicName", "name": "Medic", "autoWidth": true },
            { "data": "date", "name": "Date", "autoWidth": true },
            { "data": "time", "name": "Time", "autoWidth": true }
        ],
        "createdRow": function (row, data, dataIndex) {
            if (data['accessed'] == true) {
                $(row).addClass('Red');
            }
            if (data['accessed'] == false) {

            }
        },
        lengthMenu: [
            [10, 25, 50],
            ['10 filas', '25 filas', '50 filas']
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
            'pageLength',
            'spacer',
            {
                text: 'Recarga',
                action: function (e, dt, node, config) {
                    dt.ajax.reload();
                }
            }
        ]
    });

    oTable = $('#turns').DataTable();
    $('#btnSearch').click(function () {
        oTable.columns(4).search($('#Medics').val().trim());
        oTable.columns(5).search($('#DateTurn').val().trim());
        oTable.draw();
    });
});