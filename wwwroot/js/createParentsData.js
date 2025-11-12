// @ts-nocheck
document.addEventListener("parents:createLoaded", function() {
    appUtils.initFlatpickr("#FatherBirthDate", { maxToday: true });
    appUtils.initFlatpickr("#MotherBirthDate", { maxToday: true });
});

window.CreateParentsDataView = (function(patientId) {
    $.ajax({
        type: "GET",
        url: `/Parents/Create/${patientId}`,
        success: function(html) {
            $("#CreateFormContent").html(html);
            $("#Create").modal("show");
            document.dispatchEvent(new Event("parents:createLoaded"));
        }
    });
});

window.EditParentsDataView = (function(id) {
    $.ajax({
        type: "GET",
        url: `/Parents/Edit/${id}`,
        success: function(html) {
            $("#CreateFormContent").html(html);
            $("#Create").modal("show");
            document.dispatchEvent(new Event("parents:editLoaded"));
        }
    });
});

async function CreateParentsDataSubmit() {
    const form = document.querySelector("#CreateForm");
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const data = new FormData(form);
    const resp = await fetch("/Parents/Create", {
        method: "POST",
        headers: { "RequestVerificationToken": token },
        body: data
        });
    if (!resp.ok) {
        AppUtils.showToast("error", "Error creando datos de los padres");
        return;
    }
    $("#Create").modal("toggle");
    AppUtils.showToast("success", "Datos de los padres creados correctamente", 900);
}

async function EditParentsDataSubmit() {
    const form = document.querySelector("#CreateForm");
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const data = new FormData(form);
    const resp = await fetch("/Parents/Edit", {
        method: "POST",
        headers: { "RequestVerificationToken": token },
        body: data
        });
    if (!resp.ok) {
        AppUtils.showToast("error", "Error editando datos de los padres");
        return;
    }
    $("#Create").modal("toggle");
    AppUtils.showToast("success", "Datos de los padres editados correctamente", 900);
}
    