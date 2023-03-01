$(document).ready(function () {
    $('#Turns').DataTable({
        paging: true,
        fixedHeader: {
            header: true,
            footer: true
        },
        scrollY: 300,
        scrollCollapse: true,
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
                "previous": "Previa"
            },
            buttons: {
                pageLength: {
                    _: "Mostrando %d filas",
                    '-1': "Todas las filas"
                },
                "collection": "Exportar"
            }
        },
        responsive: true,
        dom: 'Bfrtip',
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
            'pageLength'

        ]
    });
});

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
});
