var currentPage = 1;
var pageSize = parseInt($('#pageSize').val() || -1);
var recordsTotal = 0;
var currentData = [];

(function () {
    'use strict';

    function parseDateString(s) {
        if (!s) return null;
        s = s.trim();
        let iso = /^\d{4}-\d{2}-\d{2}/;
        if (iso.test(s)) {
            let d = new Date(s);
            return isNaN(d) ? null : d;
        }
        let dm = /^(\d{1,2})\/(\d{1,2})\/(\d{4})$/;
        let m = s.match(dm);
        if (m) {
            let day = parseInt(m[1], 10);
            let month = parseInt(m[2], 10) - 1;
            let year = parseInt(m[3], 10);
            let d = new Date(year, month, day);
            return isNaN(d) ? null : d;
        }
        let d = new Date(s);
        return isNaN(d) ? null : d;
    }

    function getCellValue(row, index) {
        let cell = row.children[index];
        if (!cell) return '';
        return cell.textContent.trim();
    }

    function normalizeValue(val) {
        if (val === null || val === undefined) return { type: 'string', value: '' };
        val = String(val).trim();
        if (val === '') return { type: 'string', value: '' };

        let num = val.replace(/\s/g, '').replace(/\./g, '').replace(',', '.'); // handle 1.234,56
        if (/^-?\d+(\.\d+)?$/.test(num)) {
            let n = parseFloat(num);
            if (!isNaN(n)) return { type: 'number', value: n };
        }

        let d = parseDateString(val);
        if (d) return { type: 'date', value: d.getTime() };

        return { type: 'string', value: val.toLowerCase() };
    }

    function compareValues(a, b) {
        if (a.type === b.type) {
            if (a.value < b.value) return -1;
            if (a.value > b.value) return 1;
            return 0;
        }
        let va = String(a.value);
        let vb = String(b.value);
        return va.localeCompare(vb);
    }

    function sortTableByColumn(tableSelector, columnIndex, ascending) {
        let tbody = document.querySelector(tableSelector + ' tbody');
        if (!tbody) return;
        let rows = Array.from(tbody.querySelectorAll('tr'));
        let stabilized = rows.map((row, idx) => {
            let raw = getCellValue(row, columnIndex);
            let norm = normalizeValue(raw);
            return { row: row, norm: norm, idx: idx };
        });

        stabilized.sort(function (a, b) {
            let cmp = compareValues(a.norm, b.norm);
            if (cmp === 0) return a.idx - b.idx; 
            return ascending ? cmp : -cmp;
        });

        let fragment = document.createDocumentFragment();
        stabilized.forEach(item => fragment.appendChild(item.row));
        tbody.appendChild(fragment);
    }

    window._patientsSort = {
        parseDateString,
        normalizeValue,
        compareValues,
        sortTableByColumn
    };
})();

$(document).ready(function () {
    var sortState = { index: null, asc: true };
    const tableSelector = '#patients';

    function initHeaderSorting() {
        let buttons = document.querySelectorAll(tableSelector + ' thead .sort-btn');
        buttons.forEach(function (btn) {
            btn.removeEventListener('click', headerSortHandler);
            btn.addEventListener('click', headerSortHandler);
        });
    }

    function headerSortHandler(e) {
        var btn = e.currentTarget;
        let index = parseInt(btn.getAttribute('data-index'), 10);
        if (isNaN(index)) return;

        if (sortState.index === index) {
            sortState.asc = !sortState.asc;
        } else {
            sortState.index = index;
            sortState.asc = true;
        }

        let buttons = document.querySelectorAll(tableSelector + ' thead .sort-btn');
        buttons.forEach(function (b) {
            b.classList.remove('active');
            b.setAttribute('aria-sort', 'none');
            let ind = b.querySelector('.sort-indicator');
            if (ind) ind.textContent = '';
        });

        btn.classList.add('active');
        btn.setAttribute('aria-sort', sortState.asc ? 'ascending' : 'descending');
        let ind = btn.querySelector('.sort-indicator');
        if (ind) ind.textContent = sortState.asc ? '▲' : '▼';

        if (window._patientsSort && typeof window._patientsSort.sortTableByColumn === 'function') {
            window._patientsSort.sortTableByColumn(tableSelector, index, sortState.asc);
        } else {
            sortTableByColumnFallback(tableSelector, index, sortState.asc);
        }
    }

    function sortTableByColumnFallback(tableSelector, columnIndex, ascending) {
        var tbody = document.querySelector(tableSelector + ' tbody');
        if (!tbody) return;
        var rows = Array.from(tbody.querySelectorAll('tr'));
        rows.sort(function (a, b) {
            var va = a.children[columnIndex] ? a.children[columnIndex].textContent.trim() : '';
            var vb = b.children[columnIndex] ? b.children[columnIndex].textContent.trim() : '';
            return (va < vb ? -1 : va > vb ? 1 : 0) * (ascending ? 1 : -1);
        });
        var frag = document.createDocumentFragment();
        rows.forEach(function (r) { frag.appendChild(r); });
        tbody.appendChild(frag);
    }

    initHeaderSorting();

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
    $("#createPatient").on('click', function (event) {
        CreateView();
        $("#Create").modal('toggle');
    });

    $("#btnSearch").on('click', function () {
        currentPage = 1;
        loadData();
    });

    window.goPage = function (p) {
        currentPage = p;
        loadData();
    };

    // Tab loading logic: load partials into #tabContent and initialize visits module when present
    function loadTabContent(url, tabElement) {
        var container = document.getElementById('tabContent');
        if (!container) return;
        container.innerHTML = '<div class="text-center py-3">Cargando...</div>';
        fetch(url, { credentials: 'same-origin' })
            .then(function (response) {
                if (!response.ok) throw new Error('HTTP error ' + response.status);
                return response.text();
            })
            .then(function (html) {
                container.innerHTML = html;
                // If the loaded partial contains the visits table, initialize it
                if (typeof window.initVisitsTable === 'function') {
                    // initVisitsTable will be idempotent
                    try { window.initVisitsTable('#tabContent'); } catch (e) { console.error(e); }
                }
            })
            .catch(function (err) {
                console.error('Error loading tab content:', err);
                container.innerHTML = '<div class="text-danger p-3">Error al cargar contenido.</div>';
            });

        // Visual active class handling
        document.querySelectorAll('#myTabs .nav-link').forEach(function (t) { t.classList.remove('active'); });
        if (tabElement) tabElement.classList.add('active');
    }

    // Attach click handlers to tabs with data-url
    document.querySelectorAll('#myTabs .nav-link').forEach(function (tab) {
        if (!tab.hasAttribute('data-url')) return;
        tab.addEventListener('click', function (e) {
            e.preventDefault();
            var url = this.getAttribute('data-url');
            if (!url) return;
            loadTabContent(url, this);
        });
    });

    // Auto-load visits tab on details view if visitsTab exists
    var visitsTab = document.getElementById('visitsTab');
    if (visitsTab) {
        // simulate click to load it
        visitsTab.click();
    }
});

function loadData() {
    var search = $('#searchBox').val() || '';
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
        search,
    };

    console.debug('DataTable Request Params:', dataToSend);
    for (var k in columns) dataToSend;
    dataToSend['Columns[1][search][value]'] = search || '';
    dataToSend['columns[1][search][value]'] = search || '';
    dataToSend['Columns[2][search][value]'] = search || '';
    dataToSend['columns[2][search][value]'] = search || '';

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
}

function renderTable() {
    var $tbody = $('#patients-body');
    $tbody.empty();
    if (currentData.length === 0) {
        $tbody.append('<tr><td colspan="6" class="text-center">No se encontraron pacientes.</td></tr>');
        $('#pageInfo').text('Mostrando 0 de 0 pacientes');
        return;
    }
    currentData.forEach(function (patient) {
        var birthdate = (patient.birthDate ? (patient.birthDate.split(' ')[0] || '') : '');
        var row = '<tr>' +
            '<td>' + (patient.name || '') + '</td>' +
            '<td>' + (patient.dni || '') + '</td>' +
            '<td>' + (birthdate) + '</td>' +
            '<td>' + (patient.socialWork || '') + '</td>' +
            '<td>' + (patient.affiliateNumber || '') + '</td>' +
            '<td>' +
            '<div class="btn-group" role="group">' +
            '<button onclick="EditView(\'' + patient.id + '\');" class="btn btn-sm btn-primary me-1">Editar</button>' +
            '<a href="/Patients/Details/' + patient.id + '" class="btn btn-sm btn-secondary me-1">Detalles</a>' +
            '<button class="btn btn-sm btn-danger" onclick="confirmDelete(' + patient.id + ')">Eliminar</button>' +
            '</div>' +
            '</td>' +
            '</tr>';
        $tbody.append(row);
    });

    try {
        let buttons = document.querySelectorAll('#patients thead .sort-btn');
        buttons.forEach(function (btn) {
            btn.removeEventListener('click', function () { });
        });
    } catch (e) {
    }

    if (window && window._patientsSort) {
        var activeBtn = document.querySelector('#patients thead .sort-btn.active');
        if (activeBtn) {
            var idx = parseInt(activeBtn.getAttribute('data-index'), 10);
            var asc = activeBtn.getAttribute('aria-sort') === 'ascending';
            if (!isNaN(idx) && typeof window._patientsSort.sortTableByColumn === 'function') {
                window._patientsSort.sortTableByColumn('#patients', idx, asc);
            }
        }
    }

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

function escapeHtml(text) {
    return $('<div/>').text(text).html();
}

function CreateView() {
    $.ajax({
        type: 'GET',
        url: '/Patients/Create',
        success: function (data) {
            $("#CreateFormContent").html(data);
        }
    });
}

function EditView(id) {
    $.ajax({
        type: 'GET',
        url: '/Patients/Edit',
        data: { id: id },
        success: function (data) {
            $("#EditFormContent").html(data);
            $('#Edit').modal('toggle');
        }
    })
}