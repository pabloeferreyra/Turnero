// @ts-nocheck
(function () {
    'use strict';

    const key = "visits";
    let currentData = [];

    // -------------------------------------------
    // UTILITIES
    // -------------------------------------------
    function resolvePatientId() {
        return document.querySelector('#patientId')?.value
            || document.querySelector('#PatientId')?.value
            || '';
    }

    function parseLocalDate(raw) {
        if (!raw) return "";
        const [y, m, d] = raw.split('-').map(Number);
        return new Date(y, m - 1, d).toLocaleDateString("es-AR");
    }

    // -------------------------------------------
    // LOAD DATA
    // -------------------------------------------
    async function loadData() {
        const patientId = resolvePatientId();
        const tbody = document.querySelector('#visits-body');

        if (!tbody) return;

        if (!patientId) {
            tbody.innerHTML = '<tr><td colspan="4" class="text-center">No hay visitas para mostrar.</td></tr>';
            document.querySelector('#visits-pagination').innerHTML = '';
            document.querySelector('#pageInfoVisits').textContent = '';
            return;
        }

        const st = AppUtils.Pagination.getState(key);

        const payload = new URLSearchParams({
            draw: 1,
            start: (st.currentPage - 1) * st.pageSize,
            length: st.pageSize,
            patientId,
            "order[0][column]": st.order.column,
            "order[0][dir]": st.order.dir
        });

        const columnsMap = ['VisitDate', 'Medic', 'Reason'];
        columnsMap.forEach((name, i) => {
            payload.append(`columns[${i}][name]`, name);
        });

        const res = await fetch('/Visits/InitializeVisits', {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: payload
        });

        const response = await res.json();

        currentData =
            response?.data ||
            response?.Data ||
            [];

        st.recordsTotal =
            response?.recordsTotal ||
            response?.recordsFiltered ||
            currentData.length;

        renderTable();
        AppUtils.Sort.attachHeaderSorting("#visits", key, loadData);
        AppUtils.Pagination.renderWithState("#visits-pagination", key, loadData);
        renderPageInfo();
    }

    // -------------------------------------------
    // RENDER TABLE
    // -------------------------------------------
    function renderTable() {
        const tbody = document.querySelector('#visits-body');
        if (!tbody) return;

        tbody.innerHTML = "";

        if (!currentData.length) {
            tbody.innerHTML = '<tr><td colspan="4" class="text-center">No hay visitas para mostrar.</td></tr>';
            return;
        }

        currentData.forEach(item => {
            const id = item.id;
            const date = parseLocalDate(item.visitDate);
            const medic = typeof item.medic === "string"
                ? item.medic
                : item.medic.name;

            tbody.insertAdjacentHTML("beforeend", `
                <tr>
                    <td>${date}</td>
                    <td>${medic}</td>
                    <td>${item.reason}</td>
                    <td>
                        <button data-open-modal
                                data-modal-id="GlobalModal"
                                data-url="/Visits/Details?id=${id}"
                                data-title="Detalle de visita"
                                class="btn btn-sm btn-primary me-1">
                            Detalle
                        </button>
                    </td>
                </tr>
            `);
        });
    }

    function renderPageInfo() {
        const st = AppUtils.Pagination.getState(key);
        const el = document.querySelector('#pageInfoVisits');
        if (!el) return;

        const start = st.recordsTotal === 0
            ? 0
            : ((st.currentPage - 1) * st.pageSize) + 1;

        const end = Math.min(st.recordsTotal, st.currentPage * st.pageSize);

        el.textContent = `Mostrando ${start} - ${end} de ${st.recordsTotal} visitas`;
    }

    // -------------------------------------------
    // INIT Create Visit
    // -------------------------------------------
    function initCreateVisit() {

        const dt = document.querySelector("#VisitDate");
        if (dt) {
            AppUtils.initFlatpickr("#VisitDate", { maxToday: true });
        }

        AppUtils.FormValidationRules("#btnCreateVisit", {
            VisitDate: { required: true },
            MedicId: { required: true },
            Reason: { required: true }
        });

        const btn = document.querySelector("#btnCreateVisit");
        if (btn) {
            btn.addEventListener("click", async function (e) {
                e.preventDefault();
                if (!AppUtils.validateAll()) return;
                await ModalUtils.submitForm(
                    "GlobalModal",
                    "CreateVisitForm",
                    "/Visits/Create",
                    "POST",
                    "Nueva visita"
                );

                window.reloadVisitsTable?.();
            });
        }
    }

    function initVisitDetail() {
    }

    window.reloadVisitsTable = loadData;

    document.addEventListener("DOMContentLoaded", () => {

        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            defaultOrder: { column: 0, dir: 'asc' },
            pageSizeSelector: "#pageSize",
            onChange: loadData
        });

        const tab = document.querySelector('#visitsTab');

        if (tab) {
            tab.addEventListener('click', () => {

                document.querySelectorAll('#myTabs .nav-link')
                    .forEach(x => x.classList.remove('active'));

                tab.classList.add('active');

                const url = tab.dataset.url;
                if (!url) return;

                const container = document.querySelector('#tabContent');

                if (!document.querySelector('#visits')) {
                    fetch(url)
                        .then(r => r.text())
                        .then(html => {
                            container.innerHTML = html;
                            loadData();
                        });
                } else loadData();
            });
        }

        document.addEventListener("modal:updated", (ev) => {
            if (ev.detail.modalId !== "GlobalModal") return;

            const html = document
                .querySelector("#GlobalModal-body")
                ?.innerHTML
                ?.toLowerCase() || "";

            if (html.includes("createvisit")) {
                initCreateVisit();
            }

            if (html.includes("visit detail") || html.includes("detalle de visita")) {
                initVisitDetail();
            }
        });
    });

})();
