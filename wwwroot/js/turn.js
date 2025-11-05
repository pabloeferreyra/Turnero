(() => {
    'use strict';

    const key = "turns";


    $(document).ready(function () {

        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            pageSizeSelector: "#pageSize",
            defaultOrder: { column: 7, dir: 'asc' },
            onChange: loadData
        });

        AppUtils.Sort.attachHeaderSorting("#turns", key, loadData);

        loadData();

        window.turns_loadData = loadData;

        document.dispatchEvent(new Event("turns:loaded"));

        $('#btnSearch').click(function () {
            AppUtils.Pagination.goTo(key, 1, loadData);
        });

        $(document).on("click", "[data-create-turn]", openCreateTurn);

        $(document).on('click', '#turns-body .btn-edit', function () {
            Edit($(this).data('id'));
        });
    });

    function loadData() {

        const st = AppUtils.Pagination.getState(key);
        const order = AppUtils.Pagination.getOrder(key);

        const start = (st.currentPage - 1) * st.pageSize;
        const length = st.pageSize;

        const token = $('input[name="__RequestVerificationToken"]').val();
        const medics = $('#Medics').val();
        const dateTurn = $('#DateTurn').val();

        const payload = {
            __RequestVerificationToken: token,
            draw: 1,
            start,
            length,
            'order[0][column]': order.column,
            'order[0][dir]': order.dir
        };

        const columnsMap = ['Name', 'Dni', 'SocialWork', 'Reason', 'Medic', 'MedicId', 'Date', 'Time'];
        columnsMap.forEach((name, i) => payload[`columns[${i}][name]`] = name);

        // filtros
        payload['columns[5][search][value]'] = medics || '';
        payload['columns[6][search][value]'] = dateTurn || '';

        $.ajax({
            type: 'POST',
            url: '/Turns/InitializeTurns',
            data: payload,
            success: function (res) {

                const data = res?.data || res?.Data || [];
                const total = res?.recordsTotal || res?.recordsFiltered || data.length || 0;

                AppUtils.Pagination.setRecordsTotal(key, total);

                renderTable(data);
                AppUtils.Pagination.renderWithState("#turns-pagination", key, loadData);
            },
            error: function (xhr) {
                console.error('Error loading turns', xhr);
                $('#turns-body').html('<tr><td colspan="9" class="text-center text-danger">Error cargando turnos.</td></tr>');
                $('#turns-pagination').empty();
            }
        });
    }

    function renderTable(rows) {
        const $tbody = $('#turns-body').empty();

        if (!rows.length) {
            $tbody.html('<tr><td colspan="9" class="text-center">No hay turnos</td></tr>');
            return;
        }

        rows.forEach(d => {
            const colClass = (d.accessed === true) ? ' class="Red odd"' : '';

            let actions;

            if (d.accessed === true) {
                actions = `<span class="text-success fw-bold">Ingresado</span>`;
            }
            else if (d.isMedic === false) {
                actions = `<button data-id="${d.id}" class="btn btn-secondary btn-sm me-1 btn-edit">Editar</button>` +
                    `<button data-id="${d.id}" class="btn btn-danger btn-sm me-1 btn-delete">Eliminar</button>`;

            }
            else {
                actions = `<button data-id="${d.id}" class="btn btn-primary btn-sm me-1 btn-access">Ingreso</button>`;
            }

            $tbody.append(`
                <tr>
                  <td${colClass}>${escape(d.name)}</td>
                  <td${colClass}>${escape(d.dni)}</td>
                  <td${colClass}>${escape(d.socialWork)}</td>
                  <td${colClass}>${escape(d.reason)}</td>
                  <td${colClass}>${escape(d.medicName)}</td>
                  <td${colClass} style="display:none">${d.medicId}</td>
                  <td${colClass}>${escape(d.date)}</td>
                  <td${colClass}>${escape(d.time)}</td>
                  <td${colClass}>${actions}</td>
                </tr>
            `);
        });
    }

    function escape(t) {
        return $('<div/>').text(t || '').html();
    }

    function Edit(id) {
        $.ajax({
            type: "GET",
            url: "/Turns/Edit",
            data: { id },
            success: function (html) {
                $("#EditFormContent").html(html);
                $("#Edit").modal("show");

                document.dispatchEvent(new Event("turns:editLoaded"));
                if (window.loadData) window.loadData = loadData;
            },
            error: function () {
                AppUtils.showToast("error", "Error cargando formulario de edición.");
            }
        });
    }

    function openCreateTurn() {

        fetch("/Turns/Create", { method: "GET" })
            .then(r => r.text())
            .then(html => {
                document.querySelector("#CreateFormContent").innerHTML = html;
                $("#Create").modal("show");

                // señal para createTurn.js
                document.dispatchEvent(new Event("turns:createLoaded"));

                // aquí anclamos loadData global por si createTurn.js necesita refrescar
                window.turns_loadData = loadData;
            })
            .catch(() => {
                AppUtils.showToast("error", "Error cargando formulario de creación.");
            });
    }

    // ===== acceso =====
    let _reloadTurns = null;

    document.addEventListener("turns:loaded", function () {
        _reloadTurns = window.turns_loadData;
    });

    $(document).on('click', '#turns-body .btn-access', function () {
        const id = $(this).data('id');

        const $cell = $(this).closest('td');

        const $btn = $(this);

        $btn.addClass('btn-fade-out');

        setTimeout(() => {
            $cell.html(`
            <button data-id="${id}" class="btn btn-success btn-sm me-1 btn-acc-yes btn-fade-in">Si</button>
            <button data-id="${id}" class="btn btn-secondary btn-sm btn-acc-no btn-fade-in">No</button>
        `);

            // trigger fade-in
            requestAnimationFrame(() => {
                $cell.find('.btn-fade-in').addClass('show');
            });

        }, 250);
    });

    $(document).on('click', '#turns-body .btn-delete', function () {
        const id = $(this).data('id');

        const $cell = $(this).closest('td');
        const $btn = $(this);

        $btn.addClass('btn-fade-out');

        setTimeout(() => {
            $cell.html(`
                <button data-id="${id}" class="btn btn-danger btn-sm me-1 btn-del-yes">Si</button>
                <button data-id="${id}" class="btn btn-secondary btn-sm btn-del-no">No</button>
            `);

            // trigger fade-in
            requestAnimationFrame(() => {
                $cell.find('.btn-fade-in').addClass('show');
            });

        }, 250);
    });

    $(document).on('click', '#turns-body .btn-acc-no', function () {
        const id = $(this).data('id');
        const $cell = $(this).closest('td');
        const $btn = $(this);

        $btn.addClass('btn-fade-out');

        setTimeout(() => {
            $cell.html(`
                <button data-id="${id}" class="btn btn-primary btn-sm me-1 btn-access">Ingreso</button>
            `);

            // trigger fade-in
            requestAnimationFrame(() => {
                $cell.find('.btn-fade-in').addClass('show');
            });

        }, 250);
    });

    $(document).on('click', '#turns-body .btn-del-no', function () {
        const id = $(this).data('id');
        const $cell = $(this).closest('td');
        const $btn = $(this);

        $btn.addClass('btn-fade-out');
        setTimeout(() => {
            $cell.html(`
                <button data-id="${id}" class="btn btn-secondary btn-sm me-1 btn-edit">Editar</button>` +
                `<button data-id="${id}" class="btn btn-danger btn-sm me-1 btn-delete">Eliminar</button>
            `);

            // trigger fade-in
            requestAnimationFrame(() => {
                $cell.find('.btn-fade-in').addClass('show');
            });

        }, 250);
    });

    $(document).on('click', '#turns-body .btn-acc-yes', function () {
        const id = $(this).data('id');

        fetch("/Turns/Accessed", {
            method: "POST",
            headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
            body: new URLSearchParams({ id })
        })
            .then(r => {
                if (!r.ok) throw new Error("Post fail");

                if (typeof _reloadTurns === "function") {
                    _reloadTurns();
                }

                AppUtils.showToast("success", "Turno marcado como ingresado.");
            })
            .catch(() => AppUtils.showToast("error", "Error marcando ingreso"));
    });

    $(document).on('click', '#turns-body .btn-del-yes', function () {
        const id = $(this).data('id');

        fetch("/Turns/Delete", {
            method: "POST",
            headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
            body: new URLSearchParams({ id })
        })
            .then(r => {
                if (!r.ok) throw new Error("Post fail");

                if (typeof _reloadTurns === "function") {
                    _reloadTurns();
                }

                AppUtils.showToast("success", "Turno eliminado.");
            })
            .catch(() => AppUtils.showToast("error", "Error eliminando"));
    });

})();
document.addEventListener("turns:loaded", function () {
    AppUtils.initFlatpickr("#DateTurn", { blockSundays: true });
});

document.addEventListener("turns:editLoaded", function () {
    AppUtils.initFlatpickr("#DateTurnEdit", { minToday: true, blockSundays: true });
});

document.addEventListener("turns:createLoaded", function () {
    AppUtils.initFlatpickr("#DateTurnCreate", { minToday: true, blockSundays: true });
});