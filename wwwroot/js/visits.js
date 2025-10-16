(function () {
    'use strict';

    var currentPage = 1;
    var pageSize = parseInt($('#pageSize').val(), 10) || 25;
    var recordsTotal = 0;
    var currentData = [];
    var sortState = { index: 0, asc: true };

    var columnsMap = [
        'VisitDate',  
        'Medic',       
        'Reason'       
    ];

    $(document).ready(function () {
        loadDataVisits();

        $('#pageSize').off('change.visits').on('change.visits', function () {
            var val = parseInt($(this).val(), 10);
            if (!isNaN(val)) pageSize = val;
            currentPage = 1;
            loadDataVisits();
        });

        $(document).on('click', '#visits thead .sort-btn', function (ev) {
            ev.preventDefault();
            var idx = parseInt($(this).data('index'), 10);
            if (isNaN(idx)) idx = 0;
            if (sortState.index === idx) sortState.asc = !sortState.asc;
            else { sortState.index = idx; sortState.asc = true; }
            updateHeaderVisuals();
            currentPage = 1;
            loadDataVisits();
        });

        $(document).on('click', '#visits-body .btn-edit-visit', function () {
            var id = $(this).data('id');
            EditVisit(id);
        });
        $(document).on('click', '#visits-body .btn-delete-visit', function () {
            var id = $(this).data('id');
            DeleteVisit(id);
        });
    });

    function updateHeaderVisuals() {
        var $buttons = $('#visits thead .sort-btn');
        $buttons.each(function (i, el) {
            var $b = $(el);
            var idx = parseInt($b.data('index'), 10);
            $b.removeClass('active');
            $b.attr('aria-sort', 'none');
            $b.find('.sort-indicator').text('');
            if (idx === sortState.index) {
                $b.addClass('active');
                $b.attr('aria-sort', sortState.asc ? 'ascending' : 'descending');
                $b.find('.sort-indicator').text(sortState.asc ? '▲' : '▼');
            }
        });
    }

    function resolvePatientId() {
        var pid = $('#patientId').val() || $('#PatientId').val() || '';
        return pid || '';
    }

    function loadDataVisits() {
        var patientId = resolvePatientId();
        if (!patientId) {
            console.warn('loadDataVisits: patientId no encontrado en DOM.');
            renderEmpty();
            return;
        }

        var draw = 1;
        var start = (currentPage - 1) * (pageSize === -1 ? 0 : pageSize);
        var length = pageSize === -1 ? -1 : pageSize;
        var orderColIndex = (sortState.index !== null && !isNaN(sortState.index)) ? sortState.index : 0;
        var orderDir = sortState.asc ? 'asc' : 'desc';

        var dataToSend = {
            draw: draw,
            start: start,
            length: length,
            'order[0][column]': orderColIndex,
            'order[0][dir]': orderDir,
            patientId: patientId
        };

        for (var i = 0; i < columnsMap.length; i++) {
            dataToSend['columns[' + i + '][name]'] = columnsMap[i];
        }

        $.ajax({
            type: 'POST',
            url: '/Visits/InitializeVisits',
            data: dataToSend,
            traditional: true,
            dataType: 'json',
            success: function (response) {
                var data = response && (response.data || response.Data || response.items || response.Items || []);
                currentData = data || [];
                recordsTotal = (response && (response.recordsTotal || response.recordsFiltered)) || currentData.length || 0;

                if (length === -1) pageSize = recordsTotal;

                renderTable();
                updateHeaderVisuals();
            },
            error: function (xhr, status, err) {
                console.error('Error loading visits:', status, err, xhr && xhr.responseText);
                $('#visits-body').html('<tr><td colspan="4" class="text-center text-danger">Error al cargar los datos.</td></tr>');
                $('#visits-pagination').empty();
                $('#pageInfo').text('Mostrando 0 de 0 visitas');
            }
        });
    }

    function renderTable() {
        var $tbody = $('#visits-body');
        $tbody.empty();

        if (!currentData || currentData.length === 0) {
            $tbody.html('<tr><td colspan="4" class="text-center">No hay visitas para mostrar.</td></tr>');
            renderPagination();
            updatePageInfo();
            return;
        }

        currentData.forEach(function (item) {
            var dateRaw = item.VisitDate || item.visitDate || item.date || '';
            var date = '';
            if (dateRaw) {
                var dt = new Date(dateRaw);
                if (!isNaN(dt)) date = dt.toLocaleDateString('es-AR');
                else date = dateRaw;
            }

            var medicObj = item.Medic || item.medic || item.medico || null;
            var medicName = '';
            if (medicObj) {
                if (typeof medicObj === 'string') medicName = medicObj;
                else medicName = medicObj.name || medicObj.Name || medicObj.fullName || '';
            }

            var reason = item.Reason || item.reason || '';
            var actionsMarkup = '<button class="btn btn-sm btn-primary btn-edit-visit me-1" data-id="' + (item.id || item.Id || '') + '">Editar</button>' +
                '<button class="btn btn-sm btn-danger btn-delete-visit" data-id="' + (item.id || item.Id || '') + '">Eliminar</button>';

            var row = '<tr>' +
                '<td>' + escapeHtml(date) + '</td>' +
                '<td>' + escapeHtml(medicName) + '</td>' +
                '<td>' + escapeHtml(reason) + '</td>' +
                '<td>' + actionsMarkup + '</td>' +
                '</tr>';

            $tbody.append(row);
        });

        renderPagination();
        updatePageInfo();
    }

    function renderPagination() {
        var $pagination = $('#visits-pagination');
        $pagination.empty();

        if (pageSize === -1 || pageSize === 0) return;

        var totalPages = Math.max(1, Math.ceil(recordsTotal / pageSize));
        var startPage = Math.max(1, currentPage - 2);
        var endPage = Math.min(totalPages, currentPage + 2);

        function appendPage(p, text, active) {
            var li = $('<li>').addClass('page-item' + (active ? ' active' : ''));
            var a = $('<a href="#" class="page-link">').text(text).attr('data-page', p);
            a.on('click', function (e) {
                e.preventDefault();
                var np = parseInt($(this).data('page'), 10);
                if (!isNaN(np) && np >= 1) {
                    currentPage = np;
                    loadDataVisits();
                }
            });
            li.append(a);
            $pagination.append(li);
        }

        if (currentPage > 1) appendPage(currentPage - 1, 'Anterior', false);
        for (var p = startPage; p <= endPage; p++) appendPage(p, p, p === currentPage);
        if (currentPage < totalPages) appendPage(currentPage + 1, 'Siguiente', false);
    }

    function updatePageInfo() {
        var start = recordsTotal === 0 ? 0 : ((currentPage - 1) * (pageSize === -1 ? recordsTotal : pageSize)) + 1;
        var end = pageSize === -1 ? recordsTotal : Math.min(recordsTotal, currentPage * pageSize);
        var $pageInfo = $('#pageInfo');
        if ($pageInfo.length) $pageInfo.text('Mostrando ' + start + ' - ' + end + ' de ' + recordsTotal + ' visitas');
    }

    function renderEmpty() {
        $('#visits-body').html('<tr><td colspan="4" class="text-center">No hay visitas para mostrar.</td></tr>');
        $('#visits-pagination').empty();
        $('#pageInfo').text('');
    }

    function escapeHtml(s) {
        if (s === null || s === undefined) return '';
        return String(s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;").replace(/'/g, "&#039;");
    }

    window.EditVisit = function (id) {
        $.ajax({
            type: 'GET',
            url: '/Visits/Edit',
            data: { id: id },
            success: function (html) {
                $('#EditVisitFormContent').html(html);
                $('#EditVisit').modal('toggle');
            }
        });
    };

    window.DeleteVisit = function (id) {
        $.ajax({
            type: 'POST',
            url: '/Visits/Delete',
            data: { id: id },
            success: function () {
                Swal.fire({ position: 'top-end', icon: 'info', title: 'Visita eliminada', showConfirmButton: false, timer: 1200 });
                loadDataVisits();
            },
            error: function (xhr) {
                Swal.fire({ position: 'top-end', icon: 'error', title: 'No se pudo eliminar', showConfirmButton: false, timer: 1200 });
            }
        });
    };

    window.reloadVisitsTable = function () {
        loadDataVisits();
    };

})();
