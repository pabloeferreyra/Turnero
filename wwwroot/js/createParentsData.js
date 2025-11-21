// @ts-nocheck
(function () {
    "use strict";

    function initParentsDatePickers() {
        if (!window.AppUtils || typeof AppUtils.initFlatpickr !== "function") return;

        const father = document.querySelector("#FatherBirthDate");
        const mother = document.querySelector("#MotherBirthDate");

        if (father) AppUtils.initFlatpickr("#FatherBirthDate", { maxToday: true });
        if (mother) AppUtils.initFlatpickr("#MotherBirthDate", { maxToday: true });
    }

    function refreshParentsUI(patientId) {

        if (window.parents_loadData) {
            window.parents_loadData();
        }

        const summary = document.querySelector("#ParentsSummary");
        if (summary) {
            fetch(`/ParentsData/Summary?id=${patientId}`)
                .then(r => r.text())
                .then(html => summary.innerHTML = html);
        }
    }

    async function EditParentsDataSubmit() {

        const form = document.querySelector("#ParentsForm");
        if (!form) return;

        const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const data = new FormData(form);

        const patientId = data.get("PatientId");
        const id = data.get("Id");

        const resp = await fetch(`/ParentsData/Edit/${id}`, {
            method: "PUT",
            headers: { "RequestVerificationToken": token },
            body: data
        });

        if (!resp.ok) {
            AppUtils.showToast("error", "Error editando datos familiares");
            return;
        }

        ModalUtils.close("GlobalModal");

        AppUtils.showToast("success", "Datos familiares actualizados", 900);

        refreshParentsUI(patientId);
    }

    document.addEventListener("parents:editLoaded", function () {

        initParentsDatePickers();

        const btn = document.querySelector("#btnEditParentsData");

        if (btn) {
            btn.addEventListener("click", async (e) => {
                e.preventDefault();
                if (!AppUtils.validateAll()) return;

                await EditParentsDataSubmit();
            }, { once: true }); 
        }
    });

    document.addEventListener("parents:dataLoaded", function () {

        initParentsDatePickers();

        const btn = document.querySelector("#btnEditParentsData");

        if (btn) {
            btn.addEventListener("click", async (e) => {
                e.preventDefault();
                if (!AppUtils.validateAll()) return;

                await EditParentsDataSubmit();
            }, { once: true });
        }
    });

    window.ParentsInitDatePickers = initParentsDatePickers;

})();
