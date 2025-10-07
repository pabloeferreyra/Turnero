var currentPage = 1;
var pageSize = parseInt($('#pageSize').val()||-1);
var recordsTotal = 0;
var currentData = [];

$(document).ready(function () {
    $("#createPatient").on('click', function (event) {
        CreateView();
        $("#Create").modal('toggle');
    });
    

    // Wire up the search button from Views/Patients/Index.cshtml
    $("#btnSearch").on('click', function () {
        currentPage = 1;
        loadData();
    });
});

$(document).ready(function () {
    loadData();
    $('#searchBox').on('input', function () {
        currentPage = 1; 
        loadData();
    });
    $('#pageSize').on('change', function () {
        pageSize = parseInt($(this).val());
        currentPage = 1; 
        loadData();
    });
    $('#prevPage').on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadData();
        }
    });
    $('#nextPage').on('click', function () {
        if (currentPage * pageSize < recordsTotal) {
            currentPage++;
            loadData();
        }
    });
});

function loadData() {
    // Read the inputs from Views/Patients/Index.cshtml
    var name = $('#Name').val() || '';
    var dni = $('#Dni').val() || '';
    var form = $('#__AjaxAntiForgeryForm'); 
    var token = $('input[name="__RequestVerificationToken"]', form).val();

    // server-side paging parameters
    var draw = 1;
    var start = (currentPage - 1) * (pageSize === -1 ? 0 : pageSize);
    var length = pageSize === -1 ? -1 : pageSize;
    var columns = {
        'columns[0][data]': 'Name', 
        'columns[1][data]': 'DNI',
        'columns[2][data]': 'BirthDate',
        'columns[3][data]': 'SocialWork',
        'columns[4][data]': 'AffiliateNumber',
        'columns[5][data]': 'Actions'
    };
    var dataToSend = {
        draw: draw,
        start: start,
        length: length,
        'order[0][column]': 5, 
        'order[0][dir]': 'desc',
        Name: name,
        Dni: dni,
    };

    console.debug('DataTable Request Params:', dataToSend);
    for (var k in columns) dataToSend;
    dataToSend['Columns[1][search][value]'] = name || '';
    dataToSend['columns[1][search][value]'] = name || '';
    dataToSend['Columns[2][search][value]'] = dni || '';
    dataToSend['columns[2][search][value]'] = dni || '';

    $.ajax({
        type: "POST",
        url: "/Patients/InitializePatients",
        data: dataToSend,
        success: function (response) {
            console.debug('DataTable Response:', response);
            var data = response && (response.data || response.Data || response.items || response.Items);
            if (!data || data.length === 0) {
                if (response && response.recordsTotal && response.recordsTotal > 0) {
                    console.warn('DataTable Response indicates recordsTotal > 0 but no data found:', response);
                }
            }

            currentData = data || [];
            recordsTotal = response && (response.recordsTotal || response.recordsFiltered || currentData.length) || 0;

            if (length === -1) pageSize = recordsTotal;

            renderTable();
        },
        error: function (xhr, status, error) {
            console.error('Error loading data:', error);
            Swal.fire({ position: 'top-end', icon: 'error', title: 'Error cargando pacientes', showConfirmButton: false, timer: 1200 })
            $('#patientsTable tbody').html('<tr><td colspan="6" class="text-center text-danger">Error al cargar los datos.</td></tr>');
            $('#pageInfo').text('Mostrando 0 de 0 pacientes');
        }
    });

    function renderTable() {
        var $tbody = $('#patients-body');
        $tbody.empty();
        if (currentData.length === 0) {
            $tbody.append('<tr><td colspan="6" class="text-center">No se encontraron pacientes.</td></tr>');
            $('#pageInfo').text('Mostrando 0 de 0 pacientes');
            return;
        }
        currentData.forEach(function (patient) {
            var row = '<tr>' +
                '<td>' + (patient.name || '') + '</td>' +
                '<td>' + (patient.dni || '') + '</td>' +
                '<td>' + (patient.birthDate || '') + '</td>' +
                '<td>' + (patient.socialWork || '') + '</td>' +
                '<td>' + (patient.affiliateNumber || '') + '</td>' +
                '<td>' +
                '<a href="/Patients/Edit/' + patient.id + '" class="btn btn-sm btn-primary me-1">Editar</a>' +
                '<a href="/Patients/Details/' + patient.id + '" class="btn btn-sm btn-info me-1">Detalles</a>' +
                '<button class="btn btn-sm btn-danger" onclick="confirmDelete(' + patient.id + ')">Eliminar</button>' +
                '</td>' +
                '</tr>';
            $tbody.append(row);
        });
        renderPagination();
    }
    function renderPagination() {
        var pagination = $('#patients-pagination');
        pagination.empty();
        if (pageSize === -1 || pageSize === 0) return;
        var totalPages = Math.ceil(recordsTotal / pageSize);
        var startPage = Math.max(1, currentPage - 2);
        var endPage = Math.min(totalPages, currentPage + 2);
        if (currentPage > 1) {
            pagination.append('<li class="page-item"><a class="page-link" href="#" onclick="goPage(' + (currentPage - 1) + ');return false;">Anterior</a></li>');
        }

        for (var p = startPage; p <= endPage; p++) {
            var active = p === currentPage ? ' active' : '';
            pagination.append('<li class="page-item' + active + '"><a class="page-link" href="#" onclick="goPage(' + p + ');return false;">' + p + '</a></li>');
        }

        if (currentPage < totalPages) {
            pagination.append('<li class="page-item"><a class="page-link" href="#" onclick="goPage(' + (currentPage + 1) + ');return false;">Siguiente</a></li>');
        }
    }

    function goPage(p) {
        currentPage = p;
        loadData();
    }

    function escapeHtml(text) {
        return $('<div/>').text(text).html();
    }
}
