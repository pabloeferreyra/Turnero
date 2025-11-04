(function () {
    'use strict';

    const KEY = "visits";
    const TABLE = "#visits";
    const TBODY = "#visits-body";
    const PAGINATION = "#visits-pagination";
    const PAGEINFO = "#pageInfo";
    const PAGE_SIZE_SEL = "#pageSize";

    const URL = "/Visits/InitializeVisits";

    const columnsMap = ['VisitDate', 'Medic', 'Reason'];

    // init state + pageSize change
    AppUtils.Pagination.init(KEY, {
        pageSizeSelector: PAGE_SIZE_SEL,
        onChange: loadData
    });

    // header sorting
    AppUtils.Sort.attachHeaderSorting(TABLE, KEY, loadData);

    $(document).on('click', `${TBODY} .btn-visit-detail`, function () {
        const id = $(this).data('id');
        if (id) VisitDetail(id);
    });

    // si el tab ya está activo → cargar
    const $visitsTab = $('#visitsTab');
    if ($visitsTab.hasClass('active')) loadData();

    // cuando se clickea el tab
    $visitsTab.off('click.visits').on('click.visits', function () {
        const url = $(this).data('url');
        if (!url) return;

        if (!$(TABLE).length) {
            $('#tabContent').load(url, function () { loadData(); });
        } else {
            loadData();
        }
    });

    function resolvePatientId() {
        return $('#patientId').val() || $('#PatientId').val() || '';
    }

    // =============== core loader ===============
    function loadData() {
        const patientId = resolvePatientId();
        if (!patientId) {
            renderEmpty();
            return;
        }

        const st = AppUtils.Pagination.getState(KEY);

        const order = AppUtils.Pagination.getOrder(KEY);

        const payload = {
            draw: 1,
            start: (st.currentPage - 1) * st.pageSize,
            length: st.pageSize === -1 ? -1 : st.pageSize,
            patientId,
            'order[0][column]': order.column,
            'order[0][dir]': order.dir
        };

        columnsMap.forEach((n, i) => payload[`columns[${i}][name]`] = n);

        AppUtils.reloadTable(URL, payload, render);
    }

    // =============== render ===============
    function render(response) {
        const data = response?.data || response?.Data || [];
        const total = response?.recordsTotal || response?.recordsFiltered || data.length || 0;

        AppUtils.Pagination.setRecordsTotal(KEY, total);

        const $tbody = $(TBODY).empty();

        if (!data.length) {
            renderEmpty();
            return;
        }

        data.forEach(item => {
            const dateRaw = item.VisitDate || item.visitDate || '';
            let date = '';
            if (dateRaw) {
                const dt = new Date(dateRaw);
                date = !isNaN(dt) ? dt.toLocaleDateString('es-AR') : dateRaw;
            }

            const medicObj = item.Medic || item.medic || {};
            const medicName = typeof medicObj === 'string'
                ? medicObj
                : (medicObj.name || medicObj.Name || '');

            const reason = item.Reason || item.reason || '';
            const id = item.Id || item.id || '';

            $tbody.append(`
                <tr>
                    <td>${escapeHtml(date)}</td>
                    <td>${escapeHtml(medicName)}</td>
                    <td>${escapeHtml(reason)}</td>
                    <td><button class="btn btn-sm btn-primary btn-visit-detail" data-id="${id}">Detalle</button></td>
                </tr>
            `);
        });

        AppUtils.Pagination.renderWithState(PAGINATION, KEY, loadData);
        updatePageInfo();
    }

    function updatePageInfo() {
        const st = AppUtils.Pagination.getState(KEY);
        const total = st.recordsTotal;
        const page = st.currentPage;
        const size = st.pageSize;

        const start = total === 0 ? 0 : ((page - 1) * size) + 1;
        const end = Math.min(total, page * size);

        $(PAGEINFO).text(`Mostrando ${start} - ${end} de ${total} visitas`);
    }

    function renderEmpty() {
        $(TBODY).html('<tr><td colspan="4" class="text-center">No hay visitas para mostrar.</td></tr>');
        $(PAGINATION).empty();
        $(PAGEINFO).text('');
    }

    function escapeHtml(s) {
        return s ? String(s)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;") : '';
    }

    // global
    window.VisitDetail = function (id) {
        $.ajax({
            type: 'GET',
            url: '/Visits/Details',
            data: { id },
            success: function (html) {
                $('#VisitDetailContent').html(html);
                $('#VisitDetail').modal('show');
            }
        });
    };

    window.reloadVisitsTable = loadData;

})();
