(function () {
    'use strict';

    const key = "visits";
    let currentData = [];

    function resolvePatientId() {
        return $('#patientId').val() || $('#PatientId').val() || '';
    }

    function loadData() {

        const patientId = resolvePatientId();
        if (!patientId) {
            console.warn('patientId no encontrado');
            $('#visits-body').html('<tr><td colspan="4" class="text-center">No hay visitas para mostrar.</td></tr>');
            $('#visits-pagination').empty();
            $('#pageInfoVisits').text('');
            return;
        }

        const st = AppUtils.Pagination.getState(key);

        const payload = {
            draw: 1,
            start: (st.currentPage - 1) * st.pageSize,
            length: st.pageSize,
            patientId,
            "order[0][column]": st.order.column,
            "order[0][dir]": st.order.dir
        };

        const columnsMap = ['VisitDate', 'Medic', 'Reason'];
        columnsMap.forEach((name, i) => payload[`columns[${i}][name]`] = name);

        $.ajax({
            type: 'POST',
            url: '/Visits/InitializeVisits',
            data: payload,
            dataType: 'json',
            success: function (response) {

                currentData = response?.data || response?.Data || [];
                st.recordsTotal = response?.recordsTotal || response?.recordsFiltered || currentData.length || 0;

                renderTable();

                AppUtils.Sort.attachHeaderSorting("#visits", key, loadData);

                AppUtils.Pagination.renderWithState("#visits-pagination", key, loadData);
                renderPageInfo();
            }
        });
    }

    function renderTable() {
        const $tbody = $('#visits-body').empty();

        if (!currentData.length) {
            $tbody.html('<tr><td colspan="4" class="text-center">No hay visitas para mostrar.</td></tr>');
            return;
        }

        currentData.forEach(function (item) {
            const dateRaw = item.visitDate;
            let date = '';
            if (dateRaw) {
                let dt = new Date(dateRaw);
                date = !isNaN(dt) ? dt.toLocaleDateString('es-AR') : dateRaw;
            }

            const medicObj = item.medic;
            const medicName = typeof medicObj === 'string' ? medicObj : (medicObj.name);

            const reason = item.reason;
            const id = item.id;

            const row = `
                <tr>
                    <td>${date}</td>
                    <td>${medicName}</td>
                    <td>${reason}</td>
                    <td><button class="btn btn-sm btn-primary btn-visit-detail me-1" data-id="${id}">Detalle</button></td>
                </tr>`;
            $tbody.append(row);
        });
    }

    function renderPageInfo() {
        const st = AppUtils.Pagination.getState(key);
        const start = st.recordsTotal === 0 ? 0 : ((st.currentPage - 1) * st.pageSize) + 1;
        const end = Math.min(st.recordsTotal, st.currentPage * st.pageSize);
        $('#pageInfoVisits').text(`Mostrando ${start} - ${end} de ${st.recordsTotal} visitas`);
    }

    function VisitDetail(id) {
        $.ajax({
            type: 'GET',
            url: '/Visits/Details',
            data: { id },
            success: function (html) {
                $('#VisitDetailContent').html(html);
                $('#VisitDetail').modal('toggle');
            }
        });
    }

    function reloadVisitsTable() {
        loadData();
    }

    window.VisitDetail = VisitDetail;

    window.reloadVisitsTable = reloadVisitsTable;

    $(document).ready(function () {

        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            defaultOrder: { column: 0, dir: 'asc' },
            pageSizeSelector: '#pageSize',
            onChange: loadData
        });

        const $tab = $('#visitsTab');

        $tab.off('click.visits').on('click.visits', function () {

            // visual tabs
            $('#myTabs .nav-link').removeClass('active');
            $(this).addClass('active');

            // limpiar sort visual del tab *alergies*
            $('#allergies thead .sort-btn')
                .removeClass('active')
                .attr('aria-sort', 'none')
                .find('.sort-indicator').text('');

            const url = $(this).data('url');
            if (!url) return;

            if (!$('#visits').length) {
                $('#tabContent').load(url, function () {
                    loadData();
                });
            } else {
                loadData();
            }
        });

        $(document).on('click.visits', '#visits-body .btn-visit-detail', function () {
            const id = $(this).data('id');
            if (id) VisitDetail(id);
        });
    });

})();
