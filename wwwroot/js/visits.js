    (function () {
    'use strict';
    function createVisitsModule(rootSelector, patientIdOverride) {
        var root = rootSelector
            ? (typeof rootSelector === 'string' ? document.querySelector(rootSelector) : rootSelector)
            : document;
        if (!root) return null;

        if (root.dataset && root.dataset.visitsInit === 'true') return null;
        if (root.dataset) root.dataset.visitsInit = 'true';

        var tableSelector = '#visits';
        var $root = $(root);
        var $table = $root.find(tableSelector);
        if ($table.length === 0) return null;

        var sortState = { index: 0, asc: true };
        var currentPage = 1;
        var pageSize = parseInt($root.find('#pageSize').val(), 10) || 10;
        var recordsTotal = 0;
        var currentData = [];

        function initHeaderSorting() {
            var buttons = $table.find('thead .sort-btn');
            if (!buttons || buttons.length === 0) return;
            buttons.each(function (idx, el) {
                var b = el;
                var configuredIndex = b.hasAttribute('data-index') ? parseInt(b.getAttribute('data-index'), 10) : idx;
                b.removeEventListener('click', headerClickHandler);
                b.addEventListener('click', headerClickHandler);

                if (configuredIndex === sortState.index) {
                    b.classList.add('active');
                    b.setAttribute('aria-sort', sortState.asc ? 'ascending' : 'descending');
                    var indInit = b.querySelector('.sort-indicator');
                    if (indInit) indInit.textContent = sortState.asc ? '▲' : '▼';
                }

                function headerClickHandler(ev) {
                    ev.preventDefault();
                    if (sortState.index === configuredIndex) {
                        sortState.asc = !sortState.asc;
                    } else {
                        sortState.index = configuredIndex;
                        sortState.asc = true;
                    }
                    updateHeaderVisuals();
                    currentPage = 1;
                    loadDataVisits();
                }
            });
        }

        function updateHeaderVisuals() {
            var buttons = $table.find('thead .sort-btn');
            buttons.each(function (idx, b) {
                var configuredIndex = b.hasAttribute('data-index') ? parseInt(b.getAttribute('data-index'), 10) : idx;
                b.classList.remove('active');
                b.setAttribute('aria-sort', 'none');
                var ind = b.querySelector('.sort-indicator');
                if (ind) ind.textContent = '';
                if (configuredIndex === sortState.index) {
                    b.classList.add('active');
                    b.setAttribute('aria-sort', sortState.asc ? 'ascending' : 'descending');
                    if (ind) ind.textContent = sortState.asc ? '▲' : '▼';
                }
            });
        }

        function sortTableByColumnFallback(columnIndex, ascending) {
            var $tbody = $table.find('tbody');
            if (!$tbody || $tbody.length === 0) return;
            var rowsArr = Array.prototype.slice.call($tbody.find('tr'));
            rowsArr.sort(function (ra, rb) {
                var va = ra.children[columnIndex] ? ra.children[columnIndex].textContent.trim() : '';
                var vb = rb.children[columnIndex] ? rb.children[columnIndex].textContent.trim() : '';
                var da = Date.parse(va);
                var db = Date.parse(vb);
                if (!isNaN(da) && !isNaN(db)) return (da - db) * (ascending ? 1 : -1);
                var na = parseFloat(va.replace(',', '.'));
                var nb = parseFloat(vb.replace(',', '.'));
                if (!isNaN(na) && !isNaN(nb)) return (na - nb) * (ascending ? 1 : -1);
                if (va < vb) return ascending ? -1 : 1;
                if (va > vb) return ascending ? 1 : -1;
                return 0;
            });
            var frag = document.createDocumentFragment();
            rowsArr.forEach(function (r) { frag.appendChild(r); });
            $tbody.empty();
            $tbody.append(frag);
        }

        function renderTable() {
            var $tbody = $root.find('#visits-body');
            $tbody.empty();
            if (!currentData || currentData.length === 0) {
                $tbody.html('<tr><td colspan="4" class="text-center">No hay visitas para mostrar.</td></tr>');
            } else {
                currentData.forEach(function (item) {
                    var date = item.Date || item.date || '';
                    var medic = item.Medic || item.medic || item.Medico || '';
                    var reason = item.Reason || item.reason || '';
                    var actions = item.Actions || item.actions || '';
                    var row = '<tr>' +
                        '<td>' + escapeHtml(date) + '</td>' +
                        '<td>' + escapeHtml(medic) + '</td>' +
                        '<td>' + escapeHtml(reason) + '</td>' +
                        '<td>' + actions + '</td>' +
                        '</tr>';
                    $tbody.append(row);
                });
            }

            var $activeBtn = $table.find('thead .sort-btn.active').first();
            if ($activeBtn && $activeBtn.length) {
                var idx = parseInt($activeBtn.attr('data-index') || $activeBtn.data('index') || 0, 10);
                var asc = $activeBtn.attr('aria-sort') === 'ascending';
                if (!isNaN(idx)) sortTableByColumnFallback(idx, asc);
            }

            renderPagination();
            updatePageInfo();
        }

        function renderPagination() {
            var $pagination = $root.find('#visits-pagination');
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
            for (var p = startPage; p <= endPage; p++) {
                appendPage(p, p, p === currentPage);
            }
            if (currentPage < totalPages) appendPage(currentPage + 1, 'Siguiente', false);
        }

        function updatePageInfo() {
            var start = recordsTotal === 0 ? 0 : ((currentPage - 1) * (pageSize === -1 ? recordsTotal : pageSize)) + 1;
            var end = pageSize === -1 ? recordsTotal : Math.min(recordsTotal, currentPage * pageSize);
            var $pageInfo = $root.find('#pageInfo');
            if ($pageInfo.length) $pageInfo.text('Mostrando ' + start + ' - ' + end + ' de ' + recordsTotal + ' visitas');
        }

        function escapeHtml(text) {
            if (text === null || text === undefined) return '';
            return String(text)
                .replace(/&/g, "&amp;")
                .replace(/</g, "&lt;")
                .replace(/>/g, "&gt;")
                .replace(/\"/g, "&quot;")
                .replace(/'/g, "&#039;");
        }

        function getQueryParam(name) {
            try {
                var params = new URLSearchParams(window.location.search);
                return params.get(name);
            } catch (e) {
                return null;
            }
        }

        function resolvePatientId() {
            if (patientIdOverride !== undefined && patientIdOverride !== null && patientIdOverride !== '') {
                return String(patientIdOverride);
            }

            try {
                if (root.dataset && root.dataset.patientId) return String(root.dataset.patientId);
            } catch (e) {  }

            try {
                var dp = $root.data('patient-id');
                if (dp !== undefined && dp !== null && dp !== '') return String(dp);
            } catch (e) {  }

            try {
                var v = $root.find('#PatientId').val() || $root.find('#patientId').val() || $root.find('input[name="patientId"]').val();
                if (v !== undefined && v !== null && v !== '') return String(v);
            } catch (e) {  }

            try {
                var docV = $(document).find('#PatientId').val() || $(document).find('#patientId').val() || $(document).find('input[name="patientId"]').val();
                if (docV !== undefined && docV !== null && docV !== '') return String(docV);
            } catch (e) {  }

            try {
                var q = getQueryParam('patientId') || getQueryParam('PatientId');
                if (q !== null && q !== undefined && q !== '') return String(q);
            } catch (e) {  }

            try {
                if (window.__INITIAL_DATA__ && window.__INITIAL_DATA__.patientId) return String(window.__INITIAL_DATA__.patientId);
            } catch (e) {  }

            return '';
        }

        function loadDataVisits() {
            var draw = 1;
            var patientId = resolvePatientId() || '';
            if (!patientId) {
                console.warn('createVisitsModule: patientId no resuelto (se enviará vacío). Verifique que existe un input o data-patient-id en el DOM.');
            }

            var start = (currentPage - 1) * (pageSize === -1 ? 0 : pageSize);
            var length = pageSize === -1 ? -1 : pageSize;
            var orderColumn = (sortState.index !== null && !isNaN(sortState.index)) ? sortState.index : 0;
            var orderDir = sortState.asc ? 'asc' : 'desc';

            var dataToSend = {
                draw: draw,
                start: start,
                length: length,
                'order[0][column]': orderColumn,
                'order[0][dir]': orderDir,
                patientId: patientId
            };

            var url = '/Visits/InitializeVisits';
            if (patientId) {
                var sep = url.indexOf('?') === -1 ? '?' : '&';
                url = url + sep + 'patientId=' + encodeURIComponent(patientId);
            }

            $.ajax({
                type: 'POST',
                url: url,
                data: dataToSend,
                traditional: true, 
                dataType: 'json',
                success: function (response) {
                    var data = response && (response.data || response.Data || response.items || response.Items);
                    currentData = data || [];
                    recordsTotal = (response && (response.recordsTotal || response.recordsFiltered)) || currentData.length || 0;

                    if (length === -1) pageSize = recordsTotal;

                    renderTable();
                    updateHeaderVisuals();
                },
                error: function (xhr, status, error) {
                    console.error('Error loading visits:', status, error, xhr && xhr.responseText);
                    if (typeof Swal !== 'undefined') {
                        Swal.fire({ position: 'top-end', icon: 'error', title: 'Error cargando visitas', showConfirmButton: false, timer: 1200 });
                    }
                    $root.find(tableSelector + ' tbody').html('<tr><td colspan="4" class="text-center text-danger">Error al cargar los datos.</td></tr>');
                    var $pageInfo = $root.find('#pageInfo');
                    if ($pageInfo.length) $pageInfo.text('Mostrando 0 de 0 visitas');
                }
            });
        }

        $root.find('#pageSize').off('change.visits').on('change.visits', function () {
            var val = parseInt($(this).val(), 10);
            if (!isNaN(val)) pageSize = val;
            currentPage = 1;
            loadDataVisits();
        });

        window.goPageVisits = function (p) {
            var np = parseInt(p, 10);
            if (!isNaN(np) && np >= 1) {
                currentPage = np;
                loadDataVisits();
            }
        };

        initHeaderSorting();
        loadDataVisits();

        return {
            reload: loadDataVisits
        };
    }

    window.initVisitsTable = function (rootSelector, patientId) {
        try {
            return createVisitsModule(rootSelector, patientId);
        } catch (e) {
            console.error('initVisitsTable failed', e);
            return null;
        }
    };

    document.addEventListener('DOMContentLoaded', function () {
        var rootCandidate = document.querySelector('#tabContent') || document;
        if (document.querySelector('#visits')) {
            var pidElem = rootCandidate.querySelector('#patientId') || rootCandidate.querySelector('#PatientId') || document.querySelector('#patientId') || document.querySelector('#PatientId');
            var pid = pidElem ? pidElem.value : (rootCandidate.dataset ? rootCandidate.dataset.patientId : null);
            window.initVisitsTable(rootCandidate, pid);
        }
    });
})();

function CreateVisitView(id) {
        $.ajax({
            type: 'GET',
            url: '/Visits/Create',
            data: { id: id },
            success: function (data) {
                $('#CreateVisitFormContent').html(data);
                var $modal = $('#CreateVisit');
                if ($modal.length) {
                    $modal.modal('toggle');
                } else {
                    console.warn('Modal #CreateVisit not found in DOM');
                }
            },
            error: function (xhr, status, err) {
                console.error('Error loading Create Visit partial:', status, err, xhr && xhr.responseText);
            }
        });

}