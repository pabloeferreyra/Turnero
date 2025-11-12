// @ts-nocheck
(() => {
    'use strict';

    const key = "patients";

    $(document).ready(function () {

        // paginado unificado
        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            pageSizeSelector: "#pageSize",
            defaultOrder: { column: 0, dir: "asc" },
            onChange: loadData
        });

        // sort unificado
        AppUtils.Sort.attachHeaderSorting("#patients", key, loadData);

        // load inicial
        loadData();

        let searchTimer = null;

        $(document).on("input", "#searchBox", function () {

            clearTimeout(searchTimer);

            searchTimer = setTimeout(() => {
                AppUtils.Pagination.goTo(key, 1, loadData);
            }, 250);
        });

        $(document).on("keydown", "#searchBox", function (e) {
            if (e.key === "Enter") {
                e.preventDefault();
                AppUtils.Pagination.goTo(key, 1, loadData);
            }
        });

        $(document).on("click", "#btnSearch", function () {
            AppUtils.Pagination.goTo(key, 1, loadData);
        });

        // editar
        $(document).on("click", "#patients-body .btn-edit-patient", function () {
            EditPatient($(this).data("id"));
        });

        $(document).on("click", "[data-create-patient]", openCreatePatient);

        function openCreatePatient() {
            fetch("/Patients/Create")
                .then(r => r.text())
                .then(html => {
                    $("#CreateFormContent").html(html);
                    $("#Create").modal("show");

                    document.dispatchEvent(new Event("patients:createLoaded"));
                    window.patients_loadData = loadData;
                });
        }
    });


    function loadData() {

        const st = AppUtils.Pagination.getState(key);
        const order = AppUtils.Pagination.getOrder(key);

        const start = (st.currentPage - 1) * st.pageSize;
        const length = st.pageSize;

        const payload = {
            draw: 1,
            start,
            length,
            'order[0][column]': order.column,
            'order[0][dir]': order.dir
        };

        const search = $('#searchBox').val() || "";

        // patients columns
        const columnsMap = ['Name', 'Dni', 'BirthDate', 'SocialWork', 'AffiliateNumber'];

        columnsMap.forEach((name, i) => {
            payload[`columns[${i}][name]`] = name;
            payload[`columns[${i}][search][value]`] = search;
        });

        $.ajax({
            type: 'POST',
            url: '/Patients/InitializePatients',
            data: payload,
            dataType: "json",
            success: function (res) {

                const data = res?.data || [];
                const total = res?.recordsTotal || res?.recordsFiltered || data.length || 0;

                AppUtils.Pagination.setRecordsTotal(key, total);

                renderTable(data);
                AppUtils.Pagination.renderWithState("#patients-pagination", key, loadData);
            },
            error: function (xhr) {
                console.error('Error loading patients:', xhr);
                $('#patients-body').html('<tr><td colspan="6" class="text-center text-danger">Error cargando pacientes.</td></tr>');
            }
        });
    }

    function renderTable(rows) {

        const $tbody = $('#patients-body').empty();

        if (!rows.length) {
            $tbody.html('<tr><td colspan="6" class="text-center">No hay pacientes</td></tr>');
            return;
        }

        rows.forEach(p => {

            const birth = p.birthDate ? new Date(p.birthDate).toLocaleDateString('es-AR') : '';

            $tbody.append(`
                <tr>
                    <td>${escape(p.name)}</td>
                    <td>${escape(p.dni)}</td>
                    <td>${escape(birth)}</td>
                    <td>${escape(p.socialWork)}</td>
                    <td>${escape(p.affiliateNumber)}</td>
                    <td>
                        <div class="btn-group" role="group">
                            <button data-id="${p.id}" class="btn btn-sm btn-primary me-1 btn-edit-patient">Editar</button>
                            <a href="/Patients/Details/${p.id}" class="btn btn-sm btn-secondary me-1">Detalles</a>
                        </div>
                    </td>
                </tr>
            `);
        });
    }

    function escape(s) {
        return $('<div/>').text(s || '').html();
    }

    function EditPatient(id) {
        fetch("/Patients/Edit?id=" + id)
            .then(r => r.text())
            .then(html => {
                $("#EditFormContent").html(html);
                $("#Edit").modal("show");

                document.dispatchEvent(new Event("patients:editLoaded"));
                window.patients_loadData = loadData;
            });
    }

})();

document.addEventListener("patients:editLoaded", function () {
    AppUtils.initFlatpickr("#BirthDateEdit", { maxToday: true, blockSundays: false });
});
document.addEventListener("patients:createLoaded", () => {
    AppUtils.initFlatpickr("#BirthDate", { maxToday: true, blockSundays: false });
});