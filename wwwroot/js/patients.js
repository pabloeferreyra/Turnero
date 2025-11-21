// @ts-nocheck
(() => {
    'use strict';

    const key = "patients";

    // =======================================================
    // =============== EVENTOS GLOBALES =======================
    // =======================================================

    document.addEventListener("DOMContentLoaded", () => {

        // Inicializar búsqueda + tabla
        initTable();
        loadData();

        // Eventos CREATE / EDIT
        document.addEventListener("patients:createLoaded", initCreate);
        document.addEventListener("patients:editLoaded", initEdit);

        // Buscar por texto
        let searchTimer = null;
        document.addEventListener("input", (e) => {
            if (!e.target.matches("#searchBox")) return;
            clearTimeout(searchTimer);

            searchTimer = setTimeout(() => {
                AppUtils.Pagination.goTo(key, 1, loadData);
            }, 250);
        });

        // Enter en la búsqueda
        document.addEventListener("keydown", (e) => {
            if (e.target.matches("#searchBox") && e.key === "Enter") {
                e.preventDefault();
                AppUtils.Pagination.goTo(key, 1, loadData);
            }
        });

        // Botón búsqueda
        document.addEventListener("click", (e) => {
            if (e.target.matches("#btnSearch")) {
                AppUtils.Pagination.goTo(key, 1, loadData);
            }
        });

        // Botón "Crear"
        document.addEventListener("click", (e) => {
            const btn = e.target.closest("[data-create-patient]");
            if (!btn) return;

            ModalUtils.load("GlobalModal", "/Patients/Create", "Crear paciente");
        });

        // Botón "Editar"
        document.addEventListener("click", (e) => {
            const btn = e.target.closest("[data-edit-patient]");
            if (!btn) return;

            const id = btn.getAttribute("data-id");
            ModalUtils.load("GlobalModal", `/Patients/Edit?id=${id}`, "Editar paciente");
        });

        // Personal Background (solo abre el modal)
        document.addEventListener("click", (e) => {
            const btn = e.target.closest("[personal-background]");
            if (!btn) return;

            const id = btn.getAttribute("data-id");
            ModalUtils.load("GlobalModal", `/PersonalBackground/Index?id=${id}`, "Antecedentes personales");
        });

    });

    // =======================================================
    // =============== INIT TABLE =============================
    // =======================================================

    function initTable() {
        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            pageSizeSelector: "#pageSize",
            defaultOrder: { column: 0, dir: "asc" },
            onChange: loadData
        });

        AppUtils.Sort.attachHeaderSorting("#patients", key, loadData);
    }

    // =======================================================
    // =============== LOAD DATA =============================
    // =======================================================

    async function loadData() {

        const st = AppUtils.Pagination.getState(key);
        const order = AppUtils.Pagination.getOrder(key);

        const start = (st.currentPage - 1) * st.pageSize;
        const length = st.pageSize;
        const search = document.querySelector("#searchBox")?.value || "";

        const payload = new URLSearchParams({
            draw: 1,
            start,
            length,
            "order[0][column]": order.column,
            "order[0][dir]": order.dir
        });

        const columnsMap = ['Name', 'Dni', 'BirthDate', 'SocialWork', 'AffiliateNumber'];
        columnsMap.forEach((name, i) => {
            payload.append(`columns[${i}][name]`, name);
            payload.append(`columns[${i}][search][value]`, search);
        });

        try {
            const response = await fetch('/Patients/InitializePatients', {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: payload
            });

            if (!response.ok) throw new Error("Error cargando pacientes");

            const res = await response.json();
            const data = res?.data || [];
            const total = res?.recordsTotal || res?.recordsFiltered || data.length || 0;

            AppUtils.Pagination.setRecordsTotal(key, total);

            renderTable(data);
            AppUtils.Pagination.renderWithState("#patients-pagination", key, loadData);

        } catch (err) {
            console.error("Error loading patients:", err);

            document.querySelector("#patients-body").innerHTML =
                `<tr><td colspan="6" class="text-center text-danger">Error cargando pacientes.</td></tr>`;
        }
    }

    // =======================================================
    // =============== RENDER TABLE ==========================
    // =======================================================

    function renderTable(rows) {
        const tbody = document.querySelector("#patients-body");
        if (!tbody) return;

        if (!rows.length) {
            tbody.innerHTML =
                `<tr><td colspan="6" class="text-center">No hay pacientes</td></tr>`;
            return;
        }

        tbody.innerHTML = rows.map(p => {
            const birth = p.birthDate
                ? new Date(p.birthDate).toLocaleDateString("es-AR")
                : "";

            return `
                <tr>
                    <td>${escapeHtml(p.name)}</td>
                    <td>${escapeHtml(p.dni)}</td>
                    <td>${escapeHtml(birth)}</td>
                    <td>${escapeHtml(p.socialWork)}</td>
                    <td>${escapeHtml(p.affiliateNumber)}</td>
                    <td>
                        <div class="btn-group" role="group">
                            <button data-edit-patient 
                                    data-id="${p.id}"
                                    class="btn btn-sm btn-primary me-1">
                                Editar
                            </button>

                            <a href="/Patients/Details/${p.id}"
                               class="btn btn-sm btn-secondary me-1">
                                Detalles
                            </a>
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

    // =======================================================
    // =============== INIT CREATE ===========================
    // =======================================================

    function initCreate() {

        requestAnimationFrame(() => {

            AppUtils.initFlatpickr("#BirthDate", { maxToday: true });

            AppUtils.FormValidationRules("#btnCreatePatient", {
                Name: { required: true },
                Dni: { required: true },
                BirthDate: { required: true },
                "ContactInfo.Email": { email: true }
            });

            const btn = document.querySelector("#btnCreatePatient");
            if (!btn) return;

            btn.addEventListener("click", async (e) => {
                e.preventDefault();
                if (!AppUtils.validateAll()) return;

                await CreatePatientSubmit();
            });
        });
    }

    // =======================================================
    // =============== INIT EDIT =============================
    // =======================================================

    function initEdit() {

        requestAnimationFrame(() => {

            AppUtils.initFlatpickr("#BirthDateEdit", { maxToday: true });

            AppUtils.FormValidationRules("#btnEditPatient", {
                Name: { required: true },
                BirthDate: { required: true },
                "ContactInfo.Email": { email: true }
            });

            const btn = document.querySelector("#btnEditPatient");
            if (!btn) return;

            btn.addEventListener("click", async (e) => {
                e.preventDefault();
                if (!AppUtils.validateAll()) return;

                await EditPatientSubmit();
            });
        });
    }

    // =======================================================
    // =============== SUBMIT CREATE =========================
    // =======================================================

    async function CreatePatientSubmit() {

        await ModalUtils.submitForm(
            "GlobalModal",
            "PatientCreateForm",
            "/Patients/Create",
            "POST",
            "Pacientes"
        );

        if (window.reloadPatientsTable) {
            window.reloadPatientsTable();
        }
    }

    // =======================================================
    // =============== SUBMIT EDIT ===========================
    // =======================================================

    async function EditPatientSubmit() {

        const form = document.querySelector("#PatientEditForm");
        const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
        const data = new FormData(form);

        const resp = await fetch(`/Patients/Edit?id=${data.get("Id")}`, {
            method: "POST",
            headers: { "RequestVerificationToken": token },
            body: data
        });

        if (!resp.ok) {
            AppUtils.showToast("error", "Error editando paciente");
            return;
        }

        ModalUtils.close("GlobalModal");

        AppUtils.showToast("success", "Paciente actualizado correctamente", 900);

        if (window.reloadPatientsTable) {
            window.reloadPatientsTable();
        }
    }

    // =======================================================
    // =============== EXPOSE ================================
    // =======================================================

    window.reloadPatientsTable = loadData;

})();
