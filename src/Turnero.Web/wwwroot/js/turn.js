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

        initSearchDatePicker();

        initViewSwitcher();
        loadData();
        reloadTurns = loadData;

        document.dispatchEvent(new Event("turns:loaded"));

        const btnSearch = document.querySelector("#btnSearch");
        if (btnSearch) {
            btnSearch.addEventListener("click", () => {
                AppUtils.Pagination.goTo(key, 1, loadData);
            });
        }

        initActionButtons();
    });




    // ======================================================
    // LOAD DATA (FETCH)
    // ======================================================
    async function loadData() {

        const st = AppUtils.Pagination.getState(key);
        const order = AppUtils.Pagination.getOrder(key);
        const isAgendaView = document.querySelector("#turns-agenda")?.hidden !== true;
        const isMedicView = document.querySelector("#Medics")?.disabled === true;
        const loadAllAgendaTurns = isAgendaView && !isMedicView;

        const start = loadAllAgendaTurns ? 0 : (st.currentPage - 1) * st.pageSize;
        const length = loadAllAgendaTurns ? -1 : st.pageSize;

        const pagination = document.querySelector("#turns-pagination")?.closest("nav");
        if (pagination) pagination.hidden = loadAllAgendaTurns;

        const pageSizeControl = document.querySelector(".agenda-page-size");
        if (pageSizeControl) pageSizeControl.hidden = loadAllAgendaTurns;

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || "";

        const form = new URLSearchParams();

        form.append("__RequestVerificationToken", token);
        form.append("draw", 1);
        form.append("start", start);
        form.append("length", length);
        form.append("order[0][column]", order.column);
        form.append("order[0][dir]", order.dir);

        const columnsMap = ['Name', 'Dni', 'SocialWork', 'Reason', 'Medic', 'MedicId', 'Date', 'Time'];
        columnsMap.forEach((name, i) => form.append(`columns[${i}][name]`, name));

        const filters = getCurrentTurnFilters();
        st.filters = filters;

        Object.entries(filters).forEach(([k, v]) => {
            form.append(k.toLowerCase(), v);
        });

        try {
            const res = await fetch("/Turns/InitializeTurns", {
                method: "POST",
                body: form
            });

            const json = await res.json();
            const data = json?.data || [];
            const total = json?.recordsTotal || json?.recordsFiltered || data.length;

            AppUtils.Pagination.setRecordsTotal(key, total);
            if (!loadAllAgendaTurns) renderTable(data);
            renderAgenda(data);
            AppUtils.Pagination.renderWithState("#turns-pagination", key, loadData);

        } catch (err) {
            console.error("Error cargando turnos:", err);
            const tbody = document.querySelector("#turns-body");
            if (tbody) {
                tbody.innerHTML =
                    '<tr><td colspan="10" class="text-danger text-center">Error cargando turnos.</td></tr>';
            }
            const agenda = document.querySelector("#turns-agenda");
            if (agenda) {
                agenda.innerHTML =
                    '<div class="agenda-empty"><strong>No se pudo cargar la agenda</strong><span>Intentá nuevamente.</span></div>';
            }
        }
    }

    // ======================================================
    // AGENDA VIEW
    // ======================================================
    function initViewSwitcher() {
        const agendaButton = document.querySelector("#btnAgendaView");
        const agenda = document.querySelector("#turns-agenda");
        if (!agendaButton || !tableButton || !agenda || !table) return;

        const setView = (view) => {
            const isAgenda = view === "agenda";
            const wasAgenda = !agenda.hidden;
            agenda.hidden = !isAgenda;
            table.hidden = isAgenda;
            agendaButton.classList.toggle("active", isAgenda);
            agendaButton.setAttribute("aria-pressed", String(isAgenda));
            if (wasAgenda !== isAgenda) loadData();
        };

        agendaButton.addEventListener("click", () => setView("agenda"));
        tableButton.addEventListener("click", () => setView("table"));
        setView("agenda");
    }

    function renderAgenda(rows) {
        const agenda = document.querySelector("#turns-agenda");
        if (!agenda) return;

        const agendaDateTitle = document.querySelector("#agendaDateTitle");
        const agendaCount = document.querySelector("#agendaCount");
        const dateFilter = getCurrentTurnFilters()["Columns[6][search][value]"];

        if (agendaDateTitle) {
            agendaDateTitle.textContent = formatAgendaDate(dateFilter);
        }
        if (agendaCount) {
            agendaCount.textContent = `${rows.length} ${rows.length === 1 ? "turno" : "turnos"}`;
        }

        const isMedicView = document.querySelector("#Medics")?.disabled === true;
        agenda.classList.toggle("turns-agenda--doctors", !isMedicView);

        if (!rows.length) {
            agenda.innerHTML = `
                <div class="agenda-empty">
                    <i class="bi bi-calendar2-x" aria-hidden="true"></i>
                    <strong>No hay turnos para esta búsqueda</strong>
                    <span>Probá cambiar la fecha o el profesional.</span>
                </div>
            `;
            return;
        }

        if (!isMedicView) {
            const doctors = new Map();
            rows.forEach(row => {
                const key = String(row.medicId ?? row.medicName ?? "unknown");
                if (!doctors.has(key)) {
                    doctors.set(key, {
                        name: String(row.medicName ?? "").trim() || "Sin profesional",
                        rows: []
                    });
                }
                doctors.get(key).rows.push(row);
            });

            agenda.innerHTML = `
                <div class="agenda-doctor-grid">
                    ${[...doctors.values()]
                        .sort((a, b) => a.name.localeCompare(b.name, "es"))
                        .map((doctor, index) => `
                            <section class="agenda-doctor-column" aria-labelledby="agenda-doctor-${index}">
                                <h3 class="agenda-doctor-column__heading" id="agenda-doctor-${index}">
                                    <span>${escapeHTML(doctor.name)}</span>
                                    <span class="agenda-doctor-column__count">${doctor.rows.length} ${doctor.rows.length === 1 ? "turno" : "turnos"}</span>
                                </h3>
                                <div class="agenda-doctor-column__cards">
                                    ${doctor.rows
                                        .sort((a, b) => String(a.time ?? "").localeCompare(String(b.time ?? "")))
                                        .map(row => renderAgendaCard(row, false, true))
                                        .join("")}
                                </div>
                            </section>
                        `).join("")}
                </div>
            `;
            return;
        }

        const groups = new Map();
        rows.forEach(row => {
            const time = String(row.time ?? "").trim() || "Sin horario";
            if (!groups.has(time)) groups.set(time, []);
            groups.get(time).push(row);
        });

        const times = [...groups.keys()].sort((a, b) => a.localeCompare(b));
        agenda.innerHTML = times.map(time => {
            const cards = groups.get(time).map(row => renderAgendaCard(row, true, false)).join("");

            return `
                <section class="agenda-slot" aria-labelledby="agenda-time-${escapeHTML(time.replace(/[^0-9]/g, "") || "none")}">
                    <time class="agenda-slot__time" id="agenda-time-${escapeHTML(time.replace(/[^0-9]/g, "") || "none")}">${escapeHTML(time)}</time>
                    <div class="agenda-slot__cards">${cards}</div>
                </section>
            `;
        }).join("");
    }

    function renderAgendaCard(row, showMedic, showTime) {
        const status = row.accessed
            ? '<span class="agenda-status agenda-status--accessed"><i class="bi bi-check-circle-fill" aria-hidden="true"></i>Ingresado</span>'
            : '<span class="agenda-status"><i class="bi bi-clock" aria-hidden="true"></i>Programado</span>';

        let actions = "";
        if (!row.accessed && !row.isMedic) {
            actions = `
                <button data-open-modal data-modal-id="GlobalModal" data-url="/Turns/Edit?id=${row.id}"
                    data-title="Editar turno" class="btn btn-sm btn-primary">Editar</button>
                <button data-id="${row.id}" class="btn btn-danger btn-sm btn-delete">Eliminar</button>
            `;
        } else if (!row.accessed) {
            actions = `<button data-id="${row.id}" class="btn btn-primary btn-sm btn-access">Ingreso</button>`;
        }

        const phone = String(row.phone ?? "");
        const validPhone = /^[1-9]\d{9}$/.test(phone);
        let confirmation = `<button type="button" class="btn btn-outline-success btn-sm" disabled aria-label="WhatsApp no disponible"><i class="bi bi-whatsapp" aria-hidden="true"> No disponible</i></button>`;
        if (validPhone && row.date && row.time) {
            const url = new URL(`https://wa.me/549${phone}`);
            url.searchParams.set("text", `Hola, ¿podés confirmar tu turno del ${row.date} a las ${row.time}? Gracias.`);
            confirmation = `<a href="${escapeHTML(url.href)}" target="_blank" rel="noopener noreferrer"
                class="btn btn-outline-success btn-sm" aria-label="Solicitar confirmación por WhatsApp"><i class="bi bi-whatsapp" aria-hidden="true"> Contactar?</i></a>`;
        }

        const time = String(row.time ?? "").trim() || "Sin horario";
        return `
            <article class="agenda-card${row.accessed ? " agenda-card--accessed" : ""}${showTime ? " agenda-card--column" : ""}">
                <div class="agenda-card__identity">
                    <div class="agenda-card__name">${escapeHTML(row.name)}</div>
                    <div class="agenda-card__details">
                        DNI ${escapeHTML(row.dni)} <span aria-hidden="true">·</span> ${escapeHTML(row.socialWork)}
                    </div>
                </div>
                ${showTime ? `<time class="agenda-card__time">${escapeHTML(time)}</time>` : ""}
                <div class="agenda-card__reason"><span class="agenda-card__label">Motivo</span>${escapeHTML(row.reason)}</div>
                ${showMedic ? `<div class="agenda-card__medic"><span class="agenda-card__label">Profesional</span>${escapeHTML(row.medicName)}</div>` : ""}
                <div class="agenda-card__status">${status}</div>
                <div class="agenda-card__actions turn-actions">${actions}${confirmation}</div>
            </article>
        `;
    }

    function formatAgendaDate(dateValue) {
        if (!dateValue) return "Turnos";

        const [year, month, day] = dateValue.split("-").map(Number);
        if (!year || !month || !day) return "Turnos";

        return new Intl.DateTimeFormat("es-AR", {
            weekday: "long",
            day: "numeric",
            month: "long"
        }).format(new Date(year, month - 1, day));
    }



    // ======================================================
    // TABLE RENDER
    // ======================================================
    function renderTable(rows) {

        const tbody = document.querySelector("#turns-body");
        if (!tbody) return;

        tbody.innerHTML = "";

        if (!rows.length) {
            tbody.innerHTML = '<tr><td colspan="10" class="text-center">No hay turnos</td></tr>';
            return;
        }

        rows.forEach(d => {
            const colClass = d.accessed ? ' class="turn-cell--accessed odd"' : '';

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

            const phone = String(d.phone ?? "");
            const validPhone = /^[1-9]\d{9}$/.test(phone);
            let confirmation;
            if (validPhone && d.date && d.time) {
                const url = new URL(`https://wa.me/549${phone}`);
                url.searchParams.set("text", `Hola, ¿podés confirmar tu turno del ${d.date} a las ${d.time}? Gracias.`);
                confirmation = `
                    <a href="${escapeHTML(url.href)}" target="_blank" rel="noopener noreferrer"
                       class="btn btn-outline-success btn-sm" aria-label="Solicitar confirmación por WhatsApp (abre una nueva pestaña)">
                        <i class="bi bi-whatsapp" aria-hidden="true"> Contactar?</i>
                    </a>
                `;
            } else {
                confirmation = `
                    <button type="button" class="btn btn-outline-success btn-sm" disabled><i class="bi bi-whatsapp" aria-hidden="true"> No disponible</i></button>
                `;
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
                    <td${colClass}>${confirmation}</td>
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
            blockSundays: true,
            defaultToday: true
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
        const dateInput = document.querySelector("#DateTurnEdit");

        AppUtils.initFlatpickr("#DateTurnEdit", {
            blockSundays: true,
            defaultDate: dateInput?.value
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


    document.addEventListener("DOMContentLoaded", () => {

        const btnExcel = document.querySelector("#btnExportExcel");
        const btnPdf = document.querySelector("#btnExportPdf");
        

        if (btnExcel) {
            btnExcel.addEventListener("click", () => {
                const date = getTurnExportDate();
                AppUtils.Export.post({
                    url: "/Turns/ExportExcel",
                    filename: `turnos_${date}.xlsx`,
                    params: getCurrentTurnFilters
                });
            });
        }

        if (btnPdf) {
            btnPdf.addEventListener("click", () => {
                const date = getTurnExportDate();
                AppUtils.Export.post({
                    url: "/Turns/ExportPdf",
                    filename: `turnos_${date}.pdf`,
                    params: getCurrentTurnFilters
                });
            });
        }
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
        const cell = btn.closest(".turn-actions") || btn.closest("td");

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
        reloadTurns?.();
    }

    function restoreDelete(btn) {
        reloadTurns?.();
    }

    function confirmAccess(btn) {
        const id = btn.dataset.id;
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || "";

        const body = new URLSearchParams({ id });
        body.append("__RequestVerificationToken", token);

        fetch("/Turns/Accessed", {
            method: "POST",
            body
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
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || "";

        const body = new URLSearchParams({ id });
        body.append("__RequestVerificationToken", token);

        fetch("/Turns/Delete", {
            method: "DELETE",
            body
        })
            .then(r => {
                if (!r.ok) throw new Error();
                reloadTurns?.();
                AppUtils.showToast("success", "Turno eliminado.");
            })
            .catch(() => AppUtils.showToast("error", "Error eliminando"));
    }

    function initSearchDatePicker() {
        const el = document.querySelector("#DateTurn");
        if (!el) return;

        AppUtils.initFlatpickr("#DateTurn", {
            blockSundays: true,
            defaultToday: true
        });
    }

    function getCurrentTurnFilters() {
        const filters = {};

        const medicEl = document.querySelector("#Medics");
        const dateEl = document.querySelector("#DateTurn");

        const medic = medicEl?.value?.trim();
        let date = dateEl?.value?.trim();

        if (medic) {
            filters["Columns[5][search][value]"] = medic;
        }

        // ===============================
        // REGLA DE NEGOCIO: fecha por defecto = hoy
        // ===============================
        if (!date) {
            const today = new Date();
            const y = today.getFullYear();
            const m = String(today.getMonth() + 1).padStart(2, "0");
            const d = String(today.getDate()).padStart(2, "0");
            date = `${y}-${m}-${d}`;
        }

        filters["Columns[6][search][value]"] = date;

        return filters;
    }

    function getTurnExportDate() {
        const state = AppUtils.Pagination.getState(key);
        const dateStr = state?.filters?.["Columns[6][search][value]"];

        if (!dateStr) {
            return AppUtils.Date.ddMMyyyy();
        }

        const [y, m, d] = dateStr.split("-");
        return `${d}${m}${y}`;
    }

})();
