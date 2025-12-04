// @ts-nocheck
(function () {
    'use strict';

    const key = "vaccines";
    let currentData = [];

    function resolvePatientId() {
        return document.querySelector('#patientId')?.value
            || document.querySelector('#PatientId')?.value
            || '';
    }

    function escapeHtml(s) {
        return s ? String(s)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;") : '';
    }

    async function loadData() {

        const tbody = document.getElementById('vaccines-body');
        if (!tbody) return;

        const pid = resolvePatientId();
        if (!pid) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center">No hay vacunas para mostrar.</td></tr>';
            document.querySelector('#vaccines-pagination').innerHTML = '';
            document.querySelector('#pageInfoVaccines').textContent = '';
            return;
        }

        const st = AppUtils.Pagination.getState(key);

        const payload = new URLSearchParams({
            draw: 1,
            start: (st.currentPage - 1) * st.pageSize,
            length: st.pageSize,
            patientId: pid,
            "order[0][column]": st.order.column,
            "order[0][dir]": st.order.dir
        });

        const columnsMap = ['Description', 'DateApplied'];
        columnsMap.forEach((name, i) =>
            payload.append(`columns[${i}][name]`, name)
        );

        const res = await fetch('/Vaccines/InitializeVaccines', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: payload.toString()
        });

        const response = await res.json();
        currentData = response?.data || response?.Data || [];
        st.recordsTotal =
            response?.recordsTotal ||
            response?.recordsFiltered ||
            currentData.length ||
            0;

        renderTable();
        AppUtils.Sort.attachHeaderSorting("#vaccines", key, loadData);
        AppUtils.Pagination.renderWithState("#vaccines-pagination", key, loadData);
        renderPageInfo();
    }

    function renderTable() {
        const tbody = document.querySelector('#vaccines-body');
        tbody.innerHTML = "";
        if (!currentData.length) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center">No hay vacunas para mostrar.</td></tr>';
            return;
        }

        currentData.forEach(item => {
            const id = item.id;
            const description = escapeHtml(item.description);
            const dateApplied = escapeHtml(AppUtils.toDisplayDate(item.dateApplied));

            tbody.insertAdjacentHTML('beforeend', `
            <tr>
                <td>${description}</td>
                <td>${dateApplied}</td>
                <td><button class="btn btn-sm btn-danger me-1 btn-deleteva"
                        data-id="${id}">
                        Eliminar
                    </button>
                </td>
            </tr>
            `);
        });
    }

    function renderPageInfo() {
        const st = AppUtils.Pagination.getState(key);
        const el = document.querySelector('#pageInfoVaccines');
        if (!el) return;

        const start = st.recordsTotal === 0
            ? 0
            : ((st.currentPage - 1) * st.pageSize) + 1;

        const end = Math.min(st.recordsTotal, st.currentPage * st.pageSize);

        el.textContent = `Mostrando ${start} - ${end} de ${st.recordsTotal} vacunas`;
    }

    document.addEventListener('click', (ev) => {
        const target = ev.target;

        if (target.matches('.btn-vaccine-detail')) {
            const id = target.dataset.id;
            if (id) VaccineDetail(id);
            return;
        }

        if (target.matches('.btn-deleteva')) {
            const id = target.dataset.id;
            const cell = target.closest('td');

            target.classList.add('btn-fade-out');
            setTimeout(() => {
                cell.innerHTML = `
                    <button data-id="${id}" class="btn btn-danger btn-sm me-1 btn-confirm-deleteva">Si</button>
                    <button class="btn btn-secondary btn-sm btn-cancel-deleteva">No</button>
                `;
            }, 200);

            return;
        }

        if (target.matches('.btn-cancel-deleteva')) {
            loadData();
            return;
        }

        if (target.matches('.btn-confirm-deleteva')) {
            const id = target.dataset.id;

            fetch(`/Vaccines/Delete/${id}`, { method: "DELETE" })
                .then(r => {
                    if (!r.ok)
                        AppUtils.showToast("error", "Error eliminando vacuna");

                    loadData();
                    AppUtils.showToast("success", "vacuna eliminada");
                });

            return;
        }
    });

    // ------------------------------
    // NUEVO: inicializador del select dinámico del modal
    // ------------------------------
    function initDescriptionToggle() {
        const select = document.querySelector("#Description");
        const panel = document.querySelector("#OtherDescription");

        if (!select || !panel) return;

        const togglePanel = () => {
            panel.style.display = select.value === "Otra" ? "block" : "none";
        };

        togglePanel();
        select.addEventListener("change", togglePanel);
    }

    // Cuando el modal termina de inyectarse en el DOM
    document.addEventListener("modal:updated", () => {
        const html = document.querySelector("#GlobalModal-body")?.innerHTML ?? "";

        if (html.includes("btnCreateVaccine")) {
            initCreate();
        }

        if (html.includes("btnEditVaccine")) {
            initEdit();
        }

        // Inicializar el toggle del select dentro del modal
        if (html.includes("Description")) {
            initDescriptionToggle();
        }
    });

    function initCreate() {
        AppUtils.initFlatpickr("#DateApplied", { maxToday: true });

        AppUtils.FormValidationRules("#btnCreateVaccine", {
            Description: { required: true },
            DateApplied: { required: true }
        });

        const btn = document.querySelector("#btnCreateVaccine");
        if (btn) {
            btn.addEventListener("click", async function (e) {
                e.preventDefault();
                if (!AppUtils.validateAll()) return;

                await ModalUtils.submitForm(
                    "GlobalModal",
                    "CreateVaccineForm",
                    "/Vaccines/Create",
                    "POST",
                    "Nueva Vacuna"
                );

                loadData();
            });
        }
    }

    function initEdit() {

        let dateApplied = document.querySelector("#DateApplied")?.value;

        dateApplied = AppUtils.ddmmyyyy_to_iso(dateApplied);

        AppUtils.initFlatpickr("#DateApplied", { maxToday: true });

        AppUtils.FormValidationRules("#btnEditVaccine", {
            Description: { required: true },
            DateApplied: { required: true }
        });

        const btn = document.querySelector("#btnEditVaccine");
        if (btn) {
            btn.addEventListener("click", async function (e) {
                e.preventDefault();
                if (!AppUtils.validateAll()) return;

                await ModalUtils.submitForm(
                    "GlobalModal",
                    "CreateVaccineForm",
                    "/Vaccines/Edit",
                    "POST",
                    "Editar vacuna",
                    false
                );

                loadData();
            });
        }
    }

    // Inicialización global
    document.addEventListener("DOMContentLoaded", () => {

        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            defaultOrder: { column: 0, dir: 'asc' },
            pageSizeSelector: '#pageSizeVaccines',
            onChange: loadData
        });

        const tab = document.querySelector('#vaccinesTab');

        if (tab) {
            tab.addEventListener('click', () => {

                document.querySelectorAll('#myTabs .nav-link')
                    .forEach(x => x.classList.remove('active'));

                tab.classList.add('active');

                const url = tab.dataset.url;
                if (!url) return;

                const container = document.querySelector('#tabContent');

                if (!document.querySelector('#vaccines')) {
                    fetch(url)
                        .then(r => r.text())
                        .then(html => {
                            container.innerHTML = html;
                            loadData();
                        });
                } else {
                    loadData();
                }
            });
        }

        loadData();
    });

})();
