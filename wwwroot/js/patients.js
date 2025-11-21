// @ts-nocheck
(() => {
    'use strict';

    const key = "patients";

    document.addEventListener("DOMContentLoaded", () => {

        // Tabla: paginación + sort
        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            pageSizeSelector: "#pageSize",
            defaultOrder: { column: 0, dir: "asc" },
            onChange: loadData
        });

        AppUtils.Sort.attachHeaderSorting("#patients", key, loadData);

        loadData();

        // ---------------------------------------------------
        // SEARCH
        // ---------------------------------------------------
        let searchTimer = null;

        document.addEventListener("input", (e) => {
            if (e.target.matches("#searchBox")) {
                clearTimeout(searchTimer);
                searchTimer = setTimeout(() => {
                    AppUtils.Pagination.goTo(key, 1, loadData);
                }, 250);
            }
        });

        document.addEventListener("keydown", (e) => {
            if (e.target.matches("#searchBox") && e.key === "Enter") {
                e.preventDefault();
                AppUtils.Pagination.goTo(key, 1, loadData);
            }
        });

        document.addEventListener("click", (e) => {
            if (e.target.matches("#btnSearch")) {
                AppUtils.Pagination.goTo(key, 1, loadData);
            }
        });

        // ---------------------------------------------------
        // CLICK HANDLERS (delegación)
        // ---------------------------------------------------
        document.addEventListener("click", (e) => {

            // Editar paciente
            const btnEdit = e.target.closest("[data-edit-patient]");
            if (btnEdit) {
                const id = btnEdit.dataset.id;
                ModalUtils.load("GlobalModal", `/Patients/Edit?id=${id}`, "Editar paciente");
                return;
            }

            // Crear paciente
            const btnCreate = e.target.closest("[data-create-patient]");
            if (btnCreate) {
                ModalUtils.load("GlobalModal", "/Patients/Create", "Nuevo paciente");
                return;
            }

            // Antecedentes personales
            const btnPer = e.target.closest("[personal-background]");
            if (btnPer) {
                const id = btnPer.dataset.id;
                ModalUtils.load("GlobalModal", `/PersonalBackground/Index?id=${id}`, "Antecedentes personales");
                return;
            }

            const btnPerEdit = e.target.closest("[edit-personal-background]");
            if (btnPerEdit) {
                const id = btnPerEdit.dataset.id;
                ModalUtils.load("GlobalModal", `/PersonalBackground/Edit?id=${id}`, "Editar antecedentes personales");
                return;
            }
        });
    });

    // ======================================================
    // LOAD DATA (TABLA)
    // ======================================================
    async function loadData() {

        const st = AppUtils.Pagination.getState(key);
        const order = AppUtils.Pagination.getOrder(key);

        const start = (st.currentPage - 1) * st.pageSize;
        const length = st.pageSize;

        const search = document.querySelector("#searchBox")?.value || "";

        const payload = new URLSearchParams();
        payload.append("draw", 1);
        payload.append("start", start);
        payload.append("length", length);
        payload.append("order[0][column]", order.column);
        payload.append("order[0][dir]", order.dir);

        const columnsMap = ['Name', 'Dni', 'BirthDate', 'SocialWork', 'AffiliateNumber'];
        columnsMap.forEach((name, i) => {
            payload.append(`columns[${i}][name]`, name);
            payload.append(`columns[${i}][search][value]`, search);
        });

        try {
            const response = await fetch('/Patients/InitializePatients', {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: payload.toString()
            });

            if (!response.ok) throw new Error("Error al cargar datos");

            const res = await response.json();
            const data = res?.data || [];
            const total =
                res?.recordsTotal ||
                res?.recordsFiltered ||
                data.length ||
                0;

            AppUtils.Pagination.setRecordsTotal(key, total);
            renderTable(data);
            AppUtils.Pagination.renderWithState("#patients-pagination", key, loadData);

        } catch (err) {
            console.error("Error loading patients:", err);
            document.querySelector("#patients-body").innerHTML =
                `<tr><td colspan="6" class="text-center text-danger">Error cargando pacientes.</td></tr>`;
        }
    }

    function renderTable(rows) {

        const tbody = document.querySelector("#patients-body");
        if (!tbody) return;

        if (!rows.length) {
            tbody.innerHTML = `<tr><td colspan="6" class="text-center">No hay pacientes</td></tr>`;
            return;
        }

        tbody.innerHTML = rows.map(p => {
            const birth = p.birthDate
                ? new Date(p.birthDate).toLocaleDateString('es-AR')
                : '';

            return `
                <tr>
                    <td>${escapeHtml(p.name)}</td>
                    <td>${escapeHtml(p.dni)}</td>
                    <td>${escapeHtml(birth)}</td>
                    <td>${escapeHtml(p.socialWork)}</td>
                    <td>${escapeHtml(p.affiliateNumber)}</td>
                    <td>
                        <div class="btn-group" role="group">
                            <button data-open-modal
                                    data-modal-id="GlobalModal"
                                    data-url="/Patients/Edit?id=${p.id}"
                                    data-title="Editar paciente"
                                    class="btn btn-sm btn-primary me-1">Editar</button>

                            <a href="/Patients/Details/${p.id}" class="btn btn-sm btn-secondary me-1">Detalles</a>
                        </div>
                    </td>
                </tr>
            `;
        }).join("");
    }

    function escapeHtml(s) {
        const div = document.createElement("div");
        div.textContent = s ?? "";
        return div.innerHTML;
    }

    // ======================================================
    // CREATE / EDIT (VANILLA) — igual a allergies.js
    // ======================================================
    document.addEventListener("modal:updated", (e) => {

        const modalId = e.detail.modalId;
        if (modalId !== "GlobalModal") return;

        // Detect create
        if (document.querySelector("#PatientCreateForm")) {
            initCreate();
            return;
        }

        // Detect edit
        if (document.querySelector("#PatientEditForm")) {
            initEdit();
            return;
        }
    });

    // --------- CREATE ---------
    function initCreate() {

        const birth = document.querySelector("#BirthDate");
        if (birth) {
            AppUtils.initFlatpickr("#BirthDate", { maxToday: true });
        }

        document.addEventListener("click", handleCreateClick);
    }

    function handleCreateClick(e) {
        const btn = e.target.closest("#btnCreatePatient");
        if (!btn) return;

        e.preventDefault();

        if (!AppUtils.validateAll()) return;

        ModalUtils.submitForm(
            "GlobalModal",
            "PatientCreateForm",
            "/Patients/Create",
            "POST",
            "Nuevo paciente"
        );
    }

    // --------- EDIT ---------
    function initEdit() {

        const birth = document.querySelector("#BirthDateEdit");
        if (birth) {
            AppUtils.initFlatpickr("#BirthDateEdit", { maxToday: true });
        }

        document.addEventListener("click", handleEditClick);
    }

    function handleEditClick(e) {
        const btn = e.target.closest("#btnEditPatient");
        if (!btn) return;

        e.preventDefault();

        if (!AppUtils.validateAll()) return;

        ModalUtils.submitForm(
            "GlobalModal",
            "PatientEditForm",
            "/Patients/Edit",
            "PUT",
            "Editar paciente"
        );
    }

})();
