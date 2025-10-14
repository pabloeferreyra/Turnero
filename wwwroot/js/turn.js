var currentPage = 1;
var pageSize = -1;
var recordsTotal = 0;
var currentData = [];

$(document).ready(function () {

    loadTable();

    $('#btnSearch').click(function () {
        currentPage = 1;
        loadTable();
    });

    $('#pageSize').on('change', function () {
        pageSize = parseInt($(this).val(), 10);
        currentPage = 1;
        loadTable();
    });

    $('#btnExportExcel').on('click', function () {
        exportToExcel(currentData);
    });

    $('#btnExportPdf').on('click', function () {
        exportToPdf(currentData);
    });

    $(document).on('click', '#createTurn', function (event) {
        CreateView();
        $('#Create').modal('toggle');
    });

});

function reset() {
    loadTable();
}

function loadTable() {
    var medics = $('#Medics').val();
    var dateTurn = $('#DateTurn').val();

    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();

    var draw = 1;
    var start = (currentPage - 1) * (pageSize === -1 ? 0 : pageSize);
    var length = pageSize === -1 ? -1 : pageSize;

    var columnsLower = {
        'columns[0][name]': 'Name',
        'columns[1][name]': 'Dni',
        'columns[2][name]': 'SocialWork',
        'columns[3][name]': 'Reason',
        'columns[4][name]': 'Medic',
        'columns[5][name]': 'MedicId',
        'columns[6][name]': 'Date',
        'columns[7][name]': 'Time'
    };

    var dataToSend = {
        __RequestVerificationToken: token,
        draw: draw,
        start: start,
        length: length,
        'order[0][column]': 7,
        'order[0][dir]': 'asc'
    };

    for (var k in columnsLower) dataToSend[k] = columnsLower[k];

    dataToSend['Columns[5][search][value]'] = medics || '';
    dataToSend['columns[5][search][value]'] = medics || '';
    dataToSend['Columns[6][search][value]'] = dateTurn || '';
    dataToSend['columns[6][search][value]'] = dateTurn || '';

    console.debug('Loading turns, request payload:', dataToSend);

    $.ajax({
        type: 'POST',
        url: '/Turns/InitializeTurns',
        data: dataToSend,
        success: function (result) {
            console.debug('InitializeTurns response:', result);
            var data = result && (result.data || result.Data || result.items || result.records || []);
            if (!data || data.length === 0) {
                if (result && result.recordsTotal && result.recordsTotal > 0) {
                    console.warn('Server reports records but returned no data. Check controller paging implementation.');
                }
            }

            currentData = data || [];
            recordsTotal = result && (result.recordsTotal || result.recordsFiltered || currentData.length) || 0;

            if (length === -1) pageSize = recordsTotal;

            renderTable();
        },
        error: function (xhr) {
            console.error('Error loading turns', xhr);
            Swal.fire({ position: 'top-end', icon: 'error', title: 'Error cargando turnos', showConfirmButton: false, timer: 1200 });
        }
    });
}

function renderTable() {
    var tbody = $('#turns-body');
    tbody.empty();

    if (!currentData || currentData.length === 0) {
        tbody.append('<tr><td colspan="9" class="text-center">No hay registros</td></tr>');
        renderPagination();
        return;
    }

    currentData.forEach(function (d) {
        var actions = '';
        if (d.accessed == false && d.isMedic == false) {
            actions = '<button onclick="Edit(\'' + d.id + '\');" class="btn btn-secondary btn-sm me-1">Editar</button>' +
                '<div id="confirmaccessedSpan_' + d.id + '" style="display:none"class="btn-group" role="group">' +
                '<button onclick="Delete(\'' + d.id + '\');" class="btn btn-danger btn-sm me-1">Si</button>' +
                '<button class="btn btn-primary btn-sm" onclick="ConfirmDelete(\'' + d.id + '\', false)">No</button>' +
                '</div>' +
                '<div id="accessedSpan_' + d.id + '"class="btn-group" role="group">' +
                '<button class="btn btn-danger btn-sm" onclick="ConfirmDelete(\'' + d.id + '\', true)">Eliminar</button>'+
                '</div>';
        }
        else if (d.accessed == false && d.isMedic == true) {
            actions = '<div id="confirmaccessedSpan_' + d.id + '" style="display:none"class="btn-group" role="group">' +
                '<button onclick="Accessed(\'' + d.id + '\')" class="btn btn-danger btn-sm me-1"> Si</button>' +
                '<button class="btn btn-primary btn-sm" onclick="ConfirmAccess(\'' + d.id + '\', false)">No</button>' +
                '</div>' +
                '<div id="accessedSpan_' + d.id + '"class="btn-group" role="group">' +
                '<button href="#" class="btn btn-primary btn-sm" onclick="ConfirmAccess(\'' + d.id + '\', true)">Ingreso</button>' +
                '</div> ';
        }

        var colClass = (d.accessed == true) ? ' class="Red odd"' : '';

        var tr = '<tr>' +
            '<td' + colClass + '>' + escapeHtml(d.name || '') + '</td>' +
            '<td' + colClass + '>' + escapeHtml(d.dni || '') + '</td>' +
            '<td' + colClass + '>' + escapeHtml(d.socialWork || '') + '</td>' +
            '<td' + colClass + '>' + escapeHtml(d.reason || '') + '</td>' +
            '<td' + colClass + '>' + escapeHtml(d.medicName || '') + '</td>' +
            '<td ' + colClass + 'style="display:none">' + (d.medicId || '') + '</td>' +
            '<td' + colClass + '>' + escapeHtml(d.date || '') + '</td>' +
            '<td' + colClass + '>' + escapeHtml(d.time || '') + '</td>' +
            '<td' + colClass + '><div class="btn-group" role="group">' + actions + '</div></td>' +
            '</tr>';

        tbody.append(tr);
    });

    renderPagination();
}

function renderPagination() {
    var pagination = $('#turns-pagination');
    pagination.empty();

    if (pageSize === -1 || pageSize === 0) return;

    var totalPages = Math.ceil(recordsTotal / pageSize) || 1;
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
    loadTable();
}

function escapeHtml(text) {
    return $('<div/>').text(text).html();
}

function exportToExcel(data) {
    if (!data || data.length === 0) return;
    var ws_data = [];
    ws_data.push(["Nombre", "Dni", "Obra Social", "Motivo", "Médico", "Fecha", "Hora"]);
    data.forEach(function (d) {
        ws_data.push([d.name, d.dni, d.socialWork, d.reason, d.medicName, d.date, d.time]);
    });

    /* global XLSX */
    var wb = XLSX.utils.book_new();
    var ws = XLSX.utils.aoa_to_sheet(ws_data);
    XLSX.utils.book_append_sheet(wb, ws, "Turnos");
    XLSX.writeFile(wb, 'turnos.xls');
}

function exportToPdf(data) {
    if (!data || data.length === 0) return;
    var body = [];
    body.push(["Nombre", "Dni", "Obra Social", "Motivo", "Médico", "Fecha", "Hora"]);
    data.forEach(function (d) {
        body.push([d.name, d.dni, d.socialWork, d.reason, d.medicName, d.date, d.time]);
    });

    var docDefinition = {
        content: [
            { text: 'Turnos', style: 'header' },
            {
                table: {
                    headerRows: 1,
                    widths: ['*', 'auto', '*', '*', '*', 'auto', 'auto'],
                    body: body
                }
            }
        ],
        styles: {
            header: { fontSize: 18, bold: true, margin: [0, 0, 0, 10] }
        }
    };

    pdfMake.createPdf(docDefinition).open();
}

//-----------------------------------------------------  turnos  -----------------------------------------------------//
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
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: 'Paciente accedio correctamente',
                showConfirmButton: false,
                timer: 1200
            });
            return loadTable();
        },
        error: function (req, status, error) {
        }
    });
}

function Call(name, medicName) {
    $.ajax({
        type: 'POST',
        url: '/Turns/Call',
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
        }
    });
}

function Delete(id) {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    $.ajax({
        type: 'DELETE',
        url: '/Turns/Delete',
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
            return loadTable();
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

function CreateView() {
    $.ajax({
        type: 'GET',
        url: '/Turns/Create',
        success: function (data) {
            $("#CreateFormContent").html(data);
        }
    });
}

function Edit(id) {
    $.ajax({
        type: 'GET',
        url: '/Turns/Edit',
        data: { id: id },
        success: function (data) {
            $("#EditFormContent").html(data);
            $('#Edit').modal('toggle');
        }
    });
}

