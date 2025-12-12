// @ts-nocheck
(function () {
    'use strict';

    const key = "allergies";
    let currentData = [];

    // -----------------------------------------------------
    // UTILIDADES
    // -----------------------------------------------------
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

    // -----------------------------------------------------
    // CARGA DE TABLA
    // -----------------------------------------------------
    async function loadData() {

        const tbody = document.querySelector('#allergies-body');
        if (!tbody) return;

        const pid = resolvePatientId();
        if (!pid) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center">No hay alergias para mostrar.</td></tr>';
            document.querySelector('#allergies-pagination').innerHTML = '';
            document.querySelector('#pageInfoAllergies').textContent = '';
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

        const columnsMap = ['Name', 'Begin', 'End', 'Severity'];
        columnsMap.forEach((name, i) =>
            payload.append(`columns[${i}][name]`, name)
        );

        const res = await fetch('/Allergies/InitializeAllergies', {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: payload
        });

        const response = await res.json();

        currentData = response?.data || response?.Data || [];
        st.recordsTotal =
            response?.recordsTotal ||
            response?.recordsFiltered ||
            currentData.length ||
            0;

        renderTable();
        AppUtils.Sort.attachHeaderSorting("#allergies", key, loadData);
        AppUtils.Pagination.renderWithState("#allergies-pagination", key, loadData);
        renderPageInfo();
    }

    // -----------------------------------------------------
    // RENDER TABLA
    // -----------------------------------------------------
    function renderTable() {
        const tbody = document.querySelector('#allergies-body');
        tbody.innerHTML = "";

        if (!currentData.length) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center">No hay alergias para mostrar.</td></tr>';
            return;
        }

        currentData.forEach(item => {

            const id = item.id;
            const name = escapeHtml(item.name);
            const begin = escapeHtml(AppUtils.toDisplayDate(item.begin));
            const end = escapeHtml(AppUtils.toDisplayDate(item.end));
            const severity = escapeHtml(item.severity);

            tbody.insertAdjacentHTML('beforeend', `
                <tr>
                    <td>${name}</td>
                    <td>${begin}</td>
                    <td>${end}</td>
                    <td>${severity}</td>
                    <td>
                    <button data-open-modal
                                data-modal-id="GlobalModal"
                                data-url="/Allergies/Details?id=${id}"
                                data-title="Detalle de alergia"
                                class="btn btn-sm btn-primary me-1">
                            Detalle
                        </button>

                        <button 
                            data-open-modal
                            data-modal-id="GlobalModal"
                            data-url="/Allergies/Edit?id=${id}"
                            data-title="Editar alergia"
                            class="btn btn-sm btn-secondary me-1">
                            Editar
                        </button>

                        <button
                            class="btn btn-sm btn-danger me-1 btn-deleteal"
                            data-id="${id}">
                            Eliminar
                        </button>
                    </td>
                </tr>
            `);
        });
    }

    // -----------------------------------------------------
    // PAGE INFO
    // -----------------------------------------------------
    function renderPageInfo() {
        const st = AppUtils.Pagination.getState(key);
        const el = document.querySelector('#pageInfoAllergies');
        if (!el) return;

        const start = st.recordsTotal === 0
            ? 0
            : ((st.currentPage - 1) * st.pageSize) + 1;

        const end = Math.min(st.recordsTotal, st.currentPage * st.pageSize);

        el.textContent = `Mostrando ${start} - ${end} de ${st.recordsTotal} alergias`;
    }

   

    // -----------------------------------------------------
    // DELETE INLINE
    // -----------------------------------------------------
    document.addEventListener('click', (ev) => {
        const target = ev.target;

        if (target.matches('.btn-allergy-detail')) {
            const id = target.dataset.id;
            if (id) AllergyDetail(id);
            return;
        }

        if (target.matches('.btn-deleteal')) {
            const id = target.dataset.id;
            const cell = target.closest('td');

            target.classList.add('btn-fade-out');
            setTimeout(() => {
                cell.innerHTML = `
                    <button data-id="${id}" class="btn btn-danger btn-sm me-1 btn-confirm-deleteal">Si</button>
                    <button class="btn btn-secondary btn-sm btn-cancel-deleteal">No</button>
                `;
            }, 200);

            return;
        }

        if (target.matches('.btn-cancel-deleteal')) {
            loadData();
            return;
        }

        if (target.matches('.btn-confirm-deleteal')) {
            const id = target.dataset.id;

            fetch(`/Allergies/Delete/${id}`, { method: "DELETE" })
                .then(r => {
                    if (!r.ok)
                        AppUtils.showToast("error", "Error eliminando alergia");

                    loadData();
                    AppUtils.showToast("success", "Alergia eliminada");
                });

            return;
        }
    });

    // -----------------------------------------------------
    // MODAL UPDATED → CREATE / EDIT
    // -----------------------------------------------------
    document.addEventListener("modal:updated", () => {
        const html = document.querySelector("#GlobalModal-body")?.innerHTML ?? "";

        if (html.includes("btnCreateAllergy")) {
            initCreate();
        }

        if (html.includes("btnEditAllergy")) {
            initEdit();
        }
    });

    // -----------------------------------------------------
    // CREATE
    // -----------------------------------------------------
    function initCreate() {

        AppUtils.initFlatpickr("#Begin", { maxToday: true });
        AppUtils.initFlatpickr("#End", { maxToday: true });

        AppUtils.FormValidationRules("#btnCreateAllergy", {
            Name: { required: true },
            Begin: { required: true },
            End: {
                required: false,
                custom(value) {
                    const begin = document.querySelector("#Begin")?.value;
                    if (!begin || !value) return true;
                    if (value < begin) return "La fecha de fin no puede ser anterior al inicio.";
                    return true;
                }
            }
        });

        const btn = document.querySelector("#btnCreateAllergy");
        if (btn) {
            btn.addEventListener("click", async function (e) {
                e.preventDefault();
                if (!AppUtils.validateAll()) return;

                await ModalUtils.submitForm(
                    "GlobalModal",
                    "CreateAllergyForm",
                    "/Allergies/Create",
                    "POST",
                    "Nueva alergia"
                );

                loadData();
            });
        }
    }

    // -----------------------------------------------------
    // EDIT
    // -----------------------------------------------------
    function initEdit() {

        let begin = document.querySelector("#Begin")?.value;
        let end = document.querySelector("#End")?.value;

        begin = AppUtils.ddmmyyyy_to_iso(begin);
        end = AppUtils.ddmmyyyy_to_iso(end);

        AppUtils.initFlatpickr("#Begin", { maxToday: true, defaultDate: begin });
        AppUtils.initFlatpickr("#End", { maxToday: true, defaultDate: end });

        AppUtils.FormValidationRules("#btnEditAllergy", {
            Name: { required: true },
            Begin: { required: true },
            End: {
                required: false,
                custom(value) {
                    if (!begin || !value) return true;
                    if (value < begin) return "La fecha de fin no puede ser anterior al inicio.";
                    return true;
                }
            }
        });

        const btn = document.querySelector("#btnEditAllergy");
        if (btn) {
            btn.addEventListener("click", async function (e) {
                e.preventDefault();
                if (!AppUtils.validateAll()) return;

                await ModalUtils.submitForm(
                    "GlobalModal",
                    "CreateAllergyForm",
                    "/Allergies/Edit",
                    "POST",
                    "Editar alergia",
                    false
                );

                loadData();
            });
        }
    }

    // -----------------------------------------------------
    // INIT AL CARGAR
    // -----------------------------------------------------
    window.reloadAllergiesTable = loadData;

    document.addEventListener("DOMContentLoaded", () => {

        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            defaultOrder: { column: 0, dir: 'asc' },
            pageSizeSelector: '#pageSizeAllergies',
            onChange: loadData
        });

        const tab = document.querySelector('#allergiesTab');

        if (tab) {
            tab.addEventListener('click', () => {

                document.querySelectorAll('#myTabs .nav-link')
                    .forEach(x => x.classList.remove('active'));

                tab.classList.add('active');

                const url = tab.dataset.url;
                if (!url) return;

                const container = document.querySelector('#tabContent');

                if (!document.querySelector('#allergies')) {
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
