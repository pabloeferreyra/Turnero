// @ts-nocheck
(() => {
    'use strict';

    const TAB_ID = "parentsTab";
    const KEY = "parentsData";

    document.addEventListener("DOMContentLoaded", () => {

        const tab = document.getElementById(TAB_ID);
        if (!tab) return;

        // Cuando clickeás en "Familia"
        tab.addEventListener("click", loadParentsData);

        // Delegación de clicks dentro del tabContent
        document.addEventListener("click", (ev) => {

            // Botón "Editar datos familiares"
            const btnEdit = ev.target.closest("[data-edit-parent]");
            if (btnEdit) {
                const id = btnEdit.getAttribute("data-id");
                return loadParentsEdit(id);
            }

            // Botón "Crear datos familiares"
            const btnCreate = ev.target.closest("#btnCreateParentsData");
            if (btnCreate) {
                return submitParentsData("create");
            }

            // Botón "Guardar"
            const btnSave = ev.target.closest("#btnEditParentsData");
            if (btnSave) {
                return submitParentsData("edit");
            }
        });
    });

    function showTabLoading() {
        const container = document.getElementById("tabContent");
        if (!container) return;

        container.classList.add("tab-loading");

        let overlay = document.createElement("div");
        overlay.className = "tab-loading-overlay";
        overlay.innerHTML = `<div class="tab-loading-spinner"></div>`;
        overlay.id = "tab-loading-overlay";

        // Evitar overlays duplicados
        const old = document.getElementById("tab-loading-overlay");
        if (old) old.remove();

        container.appendChild(overlay);
    }

    function hideTabLoading() {
        const container = document.getElementById("tabContent");
        if (!container) return;

        container.classList.remove("tab-loading");

        const overlay = document.getElementById("tab-loading-overlay");
        if (overlay) overlay.remove();
    }

    // ================================================
    // LOAD TAB
    // ================================================
    function loadParentsData() {

        const tab = document.getElementById(TAB_ID);
        if (!tab) return;

        document.querySelectorAll('#myTabs .nav-link')
            .forEach(x => x.classList.remove('active'));

        tab.classList.add("active");

        const url = tab.dataset.url;
        if (!url) return;

        const container = document.getElementById("tabContent");
        if (!container) return;

        showTabLoading();

        fetch(url)
            .then(r => r.text())
            .then(html => {

                container.style.opacity = 0;

                container.innerHTML = html;

                hideTabLoading();

                requestAnimationFrame(() => {
                    container.style.transition = "opacity .25s";
                    container.style.opacity = 1;
                });

                document.dispatchEvent(new Event("parents:dataLoaded"));
                initDatePickers();
            })
            .catch(err => {
                hideTabLoading();
                console.error("Error cargando ParentsData:", err);
            });
    }

    // ================================================
    // LOAD EDIT
    // ================================================
    function loadParentsEdit(id) {

        const url = `/ParentsData/Edit?id=${encodeURIComponent(id)}`;
        const container = document.getElementById("tabContent");
        if (!container) return;

        showTabLoading();

        fetch(url)
            .then(r => r.text())
            .then(html => {

                container.style.opacity = 0;

                container.innerHTML = html;

                hideTabLoading();

                requestAnimationFrame(() => {
                    container.style.transition = "opacity .25s";
                    container.style.opacity = 1;
                });

                document.dispatchEvent(new Event("parents:editLoaded"));
                initDatePickers();
            })
            .catch(err => {
                hideTabLoading();
                console.error("Error cargando edición de ParentsData:", err);
            });
    }

    // ================================================
    // INIT DATE PICKERS
    // ================================================
    function initDatePickers() {

        if (!window.AppUtils || !AppUtils.initFlatpickr)
            return;

        const father = document.querySelector("#FatherBirthDate");
        const mother = document.querySelector("#MotherBirthDate");

        if (father)
            AppUtils.initFlatpickr("#FatherBirthDate", { maxToday: true });

        if (mother)
            AppUtils.initFlatpickr("#MotherBirthDate", { maxToday: true });
    }

    // ================================================
    // SUBMIT
    // ================================================
    async function submitParentsData(mode) {

        const form = document.querySelector("#ParentsForm");
        if (!form) return;

        if (!AppUtils.validateAll()) return;

        const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const formData = new FormData(form);

        const id = formData.get("Id");
        const patientId = formData.get("PatientId");

        // create ⇒ Id vacío
        const isCreate = (!id || id === "00000000-0000-0000-0000-000000000000");

        const url = isCreate
            ? "/ParentsData/Create"
            : `/ParentsData/Edit/${id}`;

        const method = isCreate ? "POST" : "PUT";

        const resp = await fetch(url, {
            method,
            headers: { "RequestVerificationToken": token },
            body: formData
        });

        if (!resp.ok) {
            AppUtils.showToast("error", "No se pudo guardar los datos familiares");
            return;
        }

        AppUtils.showToast("success", "Datos familiares guardados correctamente", 900);

        // Recargar vista y summary
        refreshParentsUI(patientId);
    }

    // ================================================
    // REFRESH UI
    // ================================================
    function refreshParentsUI(patientId) {

        // Recarga el tab completo
        loadParentsData();

        // Recarga el resumen arriba si existe
        const summary = document.querySelector("#ParentsSummary");
        if (summary) {
            fetch(`/ParentsData/Summary?id=${patientId}`)
                .then(r => r.text())
                .then(html => summary.innerHTML = html);
        }
    }

})();
