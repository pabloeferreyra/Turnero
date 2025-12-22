// @ts-nocheck
(() => {
    'use strict';

    const key = "patients";

    // =======================================================
    // =============== EVENTOS GLOBALES =======================
    // =======================================================

    const modalEl = document.getElementById("GlobalModal");

    if (modalEl) {
        modalEl.addEventListener("shown.bs.modal", () => {
            const body = modalEl.querySelector(".modal-body");
            if (!body) return;

            if (body.querySelector("#PatientCreateForm")) {
                initCreate();
            }

            if (body.querySelector("#PatientEditForm")) {
                initEdit();
            }
        });
    }

    document.addEventListener("DOMContentLoaded", () => {

        initTable();
        loadData();

        let searchTimer = null;
        document.addEventListener("input", (e) => {
            if (!e.target.matches("#searchBox")) return;
            clearTimeout(searchTimer);

            searchTimer = setTimeout(() => {
                AppUtils.Pagination.goTo(key, 1, loadData);
            }, 250);
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

        document.addEventListener("click", (e) => {
            const btn = e.target.closest("[data-create-patient]");
            if (!btn) return;

            ModalUtils.load("GlobalModal", "/Patients/Create", "Crear paciente");
        });

        document.addEventListener("click", (e) => {
            const btn = e.target.closest("[data-edit-patient]");
            if (!btn) return;

            const id = btn.getAttribute("data-id");
            ModalUtils.load("GlobalModal", `/Patients/Edit?id=${id}`, "Editar paciente");
        });

        document.addEventListener("click", (e) => {
            const btn = e.target.closest("[personal-background]");
            if (!btn) return;

            const id = btn.getAttribute("data-id");
            ModalUtils.load("GlobalModal", `/PersonalBackground/Index?id=${id}`, "Antecedentes personales");
        });
    });

    document.addEventListener("modal:updated", (e) => {
        const body = document.getElementById(`${e.detail.modalId}-body`);
        if (!body) return;

        if (body.querySelector("#PatientCreateForm")) {
            initCreate();
        }

        if (body.querySelector("#PatientEditForm")) {
            initEdit();
        }
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
            console.error(err);
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
                ? formatDateDMY(p.birthDate)
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

        const input = document.querySelector("#BirthDate");
        if (input && !input._flatpickr) {          // ✅ FIX
            AppUtils.initFlatpickr("#BirthDate", {
                maxToday: true,
                defaultToday: true
            });
        }

        const btn = document.querySelector("#btnCreatePatient");
        if (!btn || btn._hooked) return;            // ✅ FIX

        AppUtils.FormValidationRules("#btnCreatePatient", {
            Name: { required: true },
            Dni: { required: true },
            BirthDate: { required: true },
            "ContactInfo.Email": { email: false }
        });

        btn.addEventListener("click", async (e) => {
            e.preventDefault();
            if (!AppUtils.validateAll()) return;
            await CreatePatientSubmit();
        });

        btn._hooked = true;                         // ✅ FIX
    }

    // =======================================================
    // =============== INIT EDIT =============================
    // =======================================================

    function initEdit() {

        AppUtils.initFlatpickr("#BirthDateEdit", {
            maxToday: true
        });

        AppUtils.FormValidationRules("#btnEditPatient", {
            Name: { required: true },
            BirthDate: { required: true },
            "ContactInfo.Email": { email: true }
        });

        const btn = document.querySelector("#btnEditPatient");
        if (!btn || btn._hooked) return;

        btn.addEventListener("click", async (e) => {
            e.preventDefault();
            if (!AppUtils.validateAll()) return;
            await EditPatientSubmit();
        });

        btn._hooked = true;
    }


    // =======================================================
    // =============== SUBMITS ===============================
    // =======================================================

    async function CreatePatientSubmit() {
        await ModalUtils.submitForm(
            "GlobalModal",
            "PatientCreateForm",
            "/Patients/Create",
            "POST",
            "Pacientes"
        );

        window.reloadPatients?.();
    }

    async function EditPatientSubmit() {
        await ModalUtils.submitForm(
            "GlobalModal",
            "PatientEditForm",
            "/Patients/Edit",
            "PUT",
            "Pacientes"
        );

        window.reloadPatients?.();
    }

    function formatDateDMY(value) {
        if (!value) return "";

        // corta la hora si viene incluida
        const datePart = value.split(" ")[0];

        const parts = datePart.split("/");
        if (parts.length !== 3) return datePart;

        let [day, month, year] = parts;

        day = day.padStart(2, "0");
        month = month.padStart(2, "0");

        return `${day}/${month}/${year}`;
    }

})();
