// @ts-nocheck
(() => {
    'use strict';

    const key = "turns";
    let reloadTurns = null;

    // ======================================================
    // DOM READY
    // ======================================================
    document.addEventListener("DOMContentLoaded", () => {

        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            pageSizeSelector: "#pageSize",
            defaultOrder: { column: 7, dir: 'asc' },
            onChange: loadData
        });

        AppUtils.Sort.attachHeaderSorting("#turns", key, loadData);

        loadData();
        reloadTurns = loadData;

        document.dispatchEvent(new Event("turns:loaded"));

        // Filtro buscar
        const btnSearch = document.querySelector("#btnSearch");
        if (btnSearch) {
            btnSearch.addEventListener("click", () => {
                AppUtils.Pagination.goTo(key, 1, loadData);
            });
        }

        // Botonera ingreso/eliminación
        initActionButtons();
    });



    // ======================================================
    // LOAD DATA (FETCH)
    // ======================================================
    async function loadData() {

        const st = AppUtils.Pagination.getState(key);
        const order = AppUtils.Pagination.getOrder(key);

        const start = (st.currentPage - 1) * st.pageSize;
        const length = st.pageSize;

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || "";
        const medics = document.querySelector('#Medics')?.value ?? "";
        const dateTurn = document.querySelector('#DateTurn')?.value ?? "";

        const form = new URLSearchParams();

        form.append("__RequestVerificationToken", token);
        form.append("draw", 1);
        form.append("start", start);
        form.append("length", length);
        form.append("order[0][column]", order.column);
        form.append("order[0][dir]", order.dir);

        const columnsMap = ['Name', 'Dni', 'SocialWork', 'Reason', 'Medic', 'MedicId', 'Date', 'Time'];
        columnsMap.forEach((name, i) => form.append(`columns[${i}][name]`, name));

        form.append('columns[5][search][value]', medics);
        form.append('columns[6][search][value]', dateTurn);

        try {
            const res = await fetch("/Turns/InitializeTurns", {
                method: "POST",
                body: form
            });

            const json = await res.json();
            const data = json?.data || [];
            const total = json?.recordsTotal || json?.recordsFiltered || data.length;

            AppUtils.Pagination.setRecordsTotal(key, total);
            renderTable(data);
            AppUtils.Pagination.renderWithState("#turns-pagination", key, loadData);

        } catch (err) {
            console.error("Error cargando turnos:", err);
            const tbody = document.querySelector("#turns-body");
            if (tbody) {
                tbody.innerHTML =
                    '<tr><td colspan="9" class="text-danger text-center">Error cargando turnos.</td></tr>';
            }
        }
    }



    // ======================================================
    // TABLE RENDER
    // ======================================================
    function renderTable(rows) {

        const tbody = document.querySelector("#turns-body");
        if (!tbody) return;

        tbody.innerHTML = "";

        if (!rows.length) {
            tbody.innerHTML = '<tr><td colspan="9" class="text-center">No hay turnos</td></tr>';
            return;
        }

        rows.forEach(d => {
            const colClass = d.accessed ? ' class="Red odd"' : '';

            let actions = "";

            if (d.accessed) {
                actions = `<span class="text-success fw-bold">Ingresado</span>`;
            } else if (!d.isMedic) {
                actions = `
                    <button
                        data-open-modal
                        data-modal-id="GlobalModal"
                        data-url="/Turns/Edit?id=${d.id}"
                        data-title="Editar turno"
                        class="btn btn-sm btn-primary me-1">
                        Editar
                    </button>
                    <button data-id="${d.id}" class="btn btn-danger btn-sm me-1 btn-delete">Eliminar</button>
                `;
            } else {
                actions = `<button data-id="${d.id}" class="btn btn-primary btn-sm me-1 btn-access">Ingreso</button>`;
            }

            tbody.insertAdjacentHTML("beforeend", `
                <tr>
                    <td${colClass}>${escapeHTML(d.name)}</td>
                    <td${colClass}>${escapeHTML(d.dni)}</td>
                    <td${colClass}>${escapeHTML(d.socialWork)}</td>
                    <td${colClass}>${escapeHTML(d.reason)}</td>
                    <td${colClass}>${escapeHTML(d.medicName)}</td>
                    <td${colClass} style="display:none">${d.medicId}</td>
                    <td${colClass}>${escapeHTML(d.date)}</td>
                    <td${colClass}>${escapeHTML(d.time)}</td>
                    <td${colClass}>${actions}</td>
                </tr>
            `);
        });
    }

    function escapeHTML(t) {
        const div = document.createElement("div");
        div.textContent = t ?? "";
        return div.innerHTML;
    }


    // ======================================================
    // CREATE TURN (detectado por modal:updated)
    // ======================================================
    function initCreateTurn() {

        AppUtils.initFlatpickr("#DateTurnCreate", {
            minToday: true,
            blockSundays: true
        });

        const btn = document.querySelector("#btnCrearTurno");
        if (!btn) return;

        btn.addEventListener("click", async (e) => {
            e.preventDefault();
            if (!AppUtils.validateAll()) return;

            await ModalUtils.submitForm(
                "GlobalModal",
                "CreateForm",
                "/Turns/Create",
                "POST",
                "Nuevo turno",
                true,
                false
            );

            reloadTurns?.();
        }, { once: true });
    }

    // ======================================================
    // EDIT TURN
    // ======================================================
    function initEditTurn() {

        AppUtils.initFlatpickr("#DateTurnEdit", {
            minToday: true,
            blockSundays: true
        });

        const btn = document.querySelector("#btnEditarTurno");
        if (!btn) return;

        btn.addEventListener("click", async (e) => {
            e.preventDefault();
            if (!AppUtils.validateAll()) return;

            await ModalUtils.submitForm(
                "GlobalModal",
                "EditForm",
                "/Turns/Edit",
                "PUT",
                "Editar turno",
                true,
                false
            );

            reloadTurns?.();
        }, { once: true });
    }


    // ======================================================
    // MODAL UPDATED LISTENER — decide CREATE o EDIT automáticamente
    // ======================================================
    document.addEventListener("modal:updated", ({ detail }) => {
        if (detail.modalId !== "GlobalModal") return;

        if (document.querySelector("#DateTurnCreate")) initCreateTurn();
        if (document.querySelector("#DateTurnEdit")) initEditTurn();
    });



    // ======================================================
    // BOTONERA: ingreso / eliminar (sin jQuery)
    // ======================================================
    function initActionButtons() {

        document.addEventListener("click", (e) => {

            const target = e.target;

            // ----- INGRESAR -----
            const acc = target.closest(".btn-access");
            if (acc) return showConfirmButtons(acc, "acc");

            // ----- ELIMINAR -----
            const del = target.closest(".btn-delete");
            if (del) return showConfirmButtons(del, "del");

            // ----- REVERTIR ACCESO -----
            const accNo = target.closest(".btn-acc-no");
            if (accNo) return restoreAccess(accNo);

            // ----- REVERTIR DELETE -----
            const delNo = target.closest(".btn-del-no");
            if (delNo) return restoreDelete(delNo);

            // ----- CONFIRMAR ACCESO -----
            const accYes = target.closest(".btn-acc-yes");
            if (accYes) return confirmAccess(accYes);

            // ----- CONFIRMAR DELETE -----
            const delYes = target.closest(".btn-del-yes");
            if (delYes) return confirmDelete(delYes);
        });
    }


    // ======================================================
    // BOTONES DINÁMICOS
    // ======================================================
    function showConfirmButtons(btn, type) {
        const id = btn.dataset.id;
        const cell = btn.closest("td");

        btn.classList.add("btn-fade-out");

        setTimeout(() => {
            if (type === "acc") {
                cell.innerHTML = `
                    <button data-id="${id}" class="btn btn-success btn-sm me-1 btn-acc-yes">Si</button>
                    <button data-id="${id}" class="btn btn-secondary btn-sm btn-acc-no">No</button>
                `;
            } else {
                cell.innerHTML = `
                    <button data-id="${id}" class="btn btn-danger btn-sm me-1 btn-del-yes">Si</button>
                    <button data-id="${id}" class="btn btn-secondary btn-sm btn-del-no">No</button>
                `;
            }
        }, 250);
    }

    function restoreAccess(btn) {
        const id = btn.dataset.id;
        const cell = btn.closest("td");

        cell.innerHTML = `
            <button data-id="${id}" class="btn btn-primary btn-sm me-1 btn-access">Ingreso</button>
        `;
    }

    function restoreDelete(btn) {
        const id = btn.dataset.id;
        const cell = btn.closest("td");

        cell.innerHTML = `
            <button
                data-open-modal
                data-modal-id="GlobalModal"
                data-url="/Turns/Edit?id="${id}"
                data-title="Editar turno"
                class="btn btn-sm btn-primary me-1">
                Editar
            </button>
            <button data-id="${id}" class="btn btn-danger btn-sm me-1 btn-delete">Eliminar</button>
        `;
    }

    function confirmAccess(btn) {
        const id = btn.dataset.id;

        fetch("/Turns/Accessed", {
            method: "POST",
            headers: {
                "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: new URLSearchParams({ id })
        })
            .then(r => {
                if (!r.ok) throw new Error();
                reloadTurns?.();
                AppUtils.showToast("success", "Turno marcado como ingresado.");
            })
            .catch(() => AppUtils.showToast("error", "Error marcando ingreso"));
    }

    function confirmDelete(btn) {
        const id = btn.dataset.id;

        fetch("/Turns/Delete", {
            method: "DELETE",
            headers: {
                "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: new URLSearchParams({ id })
        })
            .then(r => {
                if (!r.ok) throw new Error();
                reloadTurns?.();
                AppUtils.showToast("success", "Turno eliminado.");
            })
            .catch(() => AppUtils.showToast("error", "Error eliminando"));
    }

})();
