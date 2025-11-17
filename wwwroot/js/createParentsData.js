// @ts-nocheck

function initParentsDatePickers() {
    if (!window.AppUtils || typeof AppUtils.initFlatpickr !== "function") return;

    if (document.querySelector("#FatherBirthDate")) {
        AppUtils.initFlatpickr("#FatherBirthDate", { maxToday: true });
    }
    if (document.querySelector("#MotherBirthDate")) {
        AppUtils.initFlatpickr("#MotherBirthDate", { maxToday: true });
    }
}

function refreshParentsUI(patientId) {

    if (window.parents_loadData) {
        window.parents_loadData();
    }

    $("#ParentsSummary").load(`/ParentsData/Summary?id=${patientId}`);
}

document.addEventListener("parents:dataLoaded", function () {

    initParentsDatePickers();

    $(document)
        .off("click.parentsCreateTab")
        .on("click.parentsCreateTab", "#btnCreateParentsData", async function (e) {
            e.preventDefault();
            if (!AppUtils.validateAll()) return;
            await CreateParentsDataSubmit();
        });

    $(document)
        .off("click.parentsEditTab")
        .on("click.parentsEditTab", "#btnEditParentsData", async function (e) {
            e.preventDefault();
            if (!AppUtils.validateAll()) return;
            await EditParentsDataSubmit();
        });
});


document.addEventListener("parents:createLoaded", function () {
    initParentsDatePickers();

    $(document)
        .off("click.parentsCreateModal")
        .on("click.parentsCreateModal", "#btnCreateParentsData", async function (e) {
            e.preventDefault();
            if (!AppUtils.validateAll()) return;
            await CreateParentsDataSubmit();
        });
});

document.addEventListener("parents:editLoaded", function () {
    initParentsDatePickers();

    $(document)
        .off("click.parentsEditModal")
        .on("click.parentsEditModal", "#btnEditParentsData", async function (e) {
            e.preventDefault();
            if (!AppUtils.validateAll()) return;
            await EditParentsDataSubmit();
        });
});

async function CreateParentsDataSubmit() {

    const form = document.querySelector("#ParentsForm");
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    const data = new FormData(form);
    const patientId = data.get("PatientId");

    const resp = await fetch("/ParentsData/Create", {
        method: "POST",
        headers: { "RequestVerificationToken": token },
        body: data
    });

    if (!resp.ok) {
        AppUtils.showToast("error", "Error creando datos familiares");
        return;
    }

    $("#Create").modal("hide");
    AppUtils.showToast("success", "Datos familiares creados correctamente", 900);

    refreshParentsUI(patientId);
}

async function EditParentsDataSubmit() {

    const form = document.querySelector("#ParentsForm");
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

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

    $("#Create").modal("hide");
    AppUtils.showToast("success", "Datos familiares actualizados", 900);

    refreshParentsUI(patientId);
}


window.ParentsInitDatePickers = initParentsDatePickers;
