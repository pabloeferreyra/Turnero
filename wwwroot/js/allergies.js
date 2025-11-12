// @ts-nocheck
(function () {
    'use strict';

    const key = "allergies";

    $(document).ready(function () {

        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            pageSizeSelector: "#pageSizeAllergies",
            defaultOrder: { column: 0, dir: 'asc' },
            onChange: loadData
        });

        AppUtils.Sort.attachHeaderSorting("#allergies", key, loadData);

        window._reloadData = loadData;

        loadData();

        const $tab = $('#allergiesTab');

        $tab.off('click.allergies').on('click.allergies', function () {

            // visual tabs
            $('#myTabs .nav-link').removeClass('active');
            $(this).addClass('active');

            // limpiar sort visual del tab “visits”
            $('#visits thead .sort-btn')
                .removeClass('active')
                .attr('aria-sort', 'none')
                .find('.sort-indicator').text('');

            const url = $(this).data('url');
            if (!url) return;

            if (!$('#allergies').length) {
                $('#tabContent').load(url, function () {
                    loadData();
                });
            } else {
                loadData();
            }
        });

        $(document).on('click.allergies', '#allergies-body .btn-allergy-detail', function () {
            const id = $(this).data('id');
            if (id) AllergyDetail(id);
        });

        $(document).on('click.allergies', '#allergies-body .btn-allergy-edit', function () {
            const id = $(this).data('id');
            if (id) AllergyEdit(id);
        });

        $(document).on('click.allergies', '#allergies-body .btn-delete', function () {
            const id = $(this).data('id');
            const $cell = $(this).closest('td');
            const $btn = $(this);

            if (!id) return;

            $btn.addClass('btn-fade-out');

            setTimeout(() => {
                $cell.html(`
                    <button data-id="${id}" class="btn btn-danger btn-sm me-1 btn-del-yes">Si</button>
                    <button data-id="${id}" class="btn btn-secondary btn-sm btn-del-no">No</button>
                `);
                requestAnimationFrame(() => { 
                    $cell.find('.btn-fade-in').addClass('show');
                });
            }, 250);

        });

        $(document).on('click', '#allergies-body .btn-del-no', function () {
            const id = $(this).data('id');
            const $cell = $(this).closest('td');
            const $btn = $(this);

            $btn.addClass('btn-fade-out');

            setTimeout(() => {
                setTimeout(() => {
                    $cell.html(`
                        <button class="btn btn-sm btn-primary btn-allergy-detail me-1" data-id="${id}">Detalle</button>
                        <button class="btn btn-sm btn-secondary btn-allergy-edit me-1" data-id="${id}">Editar</button>
                        <button data-id="${id}" class="btn btn-danger btn-sm me-1 btn-delete">Eliminar</button>
                        `);

                    requestAnimationFrame(() => {
                        $cell.find('.btn-fade-in').addClass('show');
                    });
                }, 250);
            });
        });

        $(document).on('click', '#allergies-body .btn-del-yes', function () {
            const id = $(this).data('id');

            fetch(`/Allergies/Delete/${id}`, {
                method: "DELETE",
            }).then(r => {
                if (!r.ok) AppUtils.showToast("error", "Error eliminando alergia");

                if (typeof _reloadData === "function") _reloadData();

                AppUtils.showToast("success", "Alergia eliminada correctamente");
            });
        });
    });

    function resolvePatientId() {
        return $('#patientId').val() || $('#PatientId').val() || '';
    }

    window.AllergyDetail = function (id) {
        $.ajax({
            type: 'GET',
            url: '/Allergies/Details',
            data: { id },
            success: function (html) {
                $('#AllergyDetailContent').html(html);
                $('#AllergyDetail').modal('toggle');
            }
        });
    }

    window.AllergyEdit = function (id) {
        $.ajax({
            type: 'GET',
            url: '/Allergies/Edit',
            data: { id },
            success: function (html) {
                $('#CreateAllergyFormContent').html(html);
                $('#CreateAllergy').modal('show');
                document.dispatchEvent(new Event("allergies:editLoaded"));
            }
        });
    }

    function loadData() {
        const pid = resolvePatientId();
        if (!pid) {
            renderEmpty();
            return;
        }

        const st = AppUtils.Pagination.getState(key);
        const order = AppUtils.Pagination.getOrder(key);

        const start = (st.currentPage - 1) * st.pageSize;
        const length = st.pageSize;

        const payload = {
            draw: 1,
            start,
            length,
            'order[0][column]': order.column,
            'order[0][dir]': order.dir,
            patientId: pid
        };

        const columnsMap = ['Name', 'Begin', 'End', 'Severity'];
        columnsMap.forEach((name, i) => payload[`columns[${i}][name]`] = name);

        $.ajax({
            type: "POST",
            url: "/Allergies/InitializeAllergies",
            data: payload,
            dataType: "json",
            success: function (response) {
                const data = response?.data || response?.Data || [];
                const total = response?.recordsTotal || response?.recordsFiltered || data.length || 0;

                AppUtils.Pagination.setRecordsTotal(key, total);

                renderTable(data);
                AppUtils.Pagination.renderWithState("#allergies-pagination", key, loadData);
                updatePageInfo(total);
            },
            error: function (xhr) {
                console.error("Error cargando alergias:", xhr);
                renderEmpty();
            }
        });
    }

    function renderTable(rows) {
        const $tbody = $('#allergies-body').empty();

        if (!rows.length) {
            renderEmpty();
            return;
        }

        for (const item of rows) {
            const id = item.id;
            const name = item.name;
            const begin = toDisplayDate(item.begin);
            const end = toDisplayDate(item.end);
            const severity = item.severity;

            $tbody.append(`
                <tr>
                    <td>${escape(name)}</td>
                    <td>${escape(begin)}</td>
                    <td>${escape(end)}</td>
                    <td>${escape(severity)}</td>
                    <td><div class="btn-group"><button class="btn btn-sm btn-primary btn-allergy-detail me-1" data-id="${id}">Detalle</button>
                    <button class="btn btn-sm btn-secondary btn-allergy-edit me-1" data-id="${id}">Editar</button>
                    <button data-id="${id}" class="btn btn-danger btn-sm me-1 btn-delete">Eliminar</button></div></td>
                </tr>
            `);
        }
    }

    function toDisplayDate(raw) {
        if (!raw) return "";
        const d = new Date(raw);
        return !isNaN(d) ? d.toLocaleDateString('es-AR') : raw;
    }

    function escape(s) {
        return s ? String(s)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;") : '';
    }

    function renderEmpty() {
        $('#allergies-body').html('<tr><td colspan="5" class="text-center">No hay alergias para mostrar.</td></tr>');
        $('#allergies-pagination').empty();
        $('#pageInfoAllergies').text('');
    }

    function updatePageInfo(total) {
        const st = AppUtils.Pagination.getState(key);
        const start = total === 0 ? 0 : ((st.currentPage - 1) * st.pageSize) + 1;
        const end = Math.min(total, st.currentPage * st.pageSize);
        $('#pageInfoAllergies').text(`Mostrando ${start} - ${end} de ${total} alergias`);
    }

})();
