// @ts-nocheck

document.addEventListener("parents:editLoaded", function () {

    initParentsDatePickers();

    $(document)
        .off("click.parentsEditModal")
        .on("click.parentsEditModal", "#btnSubmitParentsEdit", async function (e) {
            e.preventDefault();
            if (!AppUtils.validateAll()) return;
            await EditParentsDataSubmit();
        });
});

async function EditParentsDataSubmit() {

    const form = document.querySelector("#CreateParentsForm");
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    const data = new FormData(form);

    const resp = await fetch("/ParentsData/Edit", {
        method: "POST",
        headers: {
            "RequestVerificationToken": token
        },
        body: data
    });

    if (!resp.ok) {
        AppUtils.showToast("error", "Error editando datos de los padres");
        return;
    }

    $("#Create").modal("toggle");
    AppUtils.showToast("success", "Datos familiares actualizados!", 900);

    if (window.parents_loadData)
        window.parents_loadData();
}

window.EditParentsDataView = function (id) {
    $.get(`/ParentsData/Edit/${id}`, function (html) {
        $("#CreateFormContent").html(html);
        $("#Create").modal("show");

        document.dispatchEvent(new Event("parents:editLoaded"));
    });
};
