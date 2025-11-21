// @ts-nocheck

// Delegación para botón "Crear"
document.addEventListener("click", async (ev) => {
    const btn = ev.target.closest("#btnCreateAllergy");
    if (!btn) return;

    ev.preventDefault();
    if (!AppUtils.validateAll()) return;
    await CreateAllergySubmit();
});


// Delegación para botón "Editar"
document.addEventListener("click", async (ev) => {
    const btn = ev.target.closest("#btnEditAllergy");
    if (!btn) return;

    ev.preventDefault();
    if (!AppUtils.validateAll()) return;
    await EditAllergySubmit();
});


// -------------------------------
// CREATE SUBMIT
// -------------------------------
async function CreateAllergySubmit() {

    ModalUtils.submitForm(
        "GlobalModal",
        "CreateAllergyForm",
        "/Allergies/Create",
        "POST",
        "Alergias"
    );

    if (window.reloadAllergiesTable) {
        window.reloadAllergiesTable();
    }
}


// -------------------------------
// EDIT SUBMIT
// -------------------------------
async function EditAllergySubmit() {

    const form = document.querySelector("#CreateAllergyForm");
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const data = new FormData(form);

    const resp = await fetch("/Allergies/Edit", {
        method: "POST",
        headers: {
            "RequestVerificationToken": token
        },
        body: data
    });

    if (!resp.ok) {
        AppUtils.showToast("error", "Error editando alergia");
        return;
    }

    bootstrap.Modal.getOrCreateInstance("#CreateAllergy").toggle();

    AppUtils.showToast("success", "Alergia editada correctamente", 900);

    if (window.reloadAllergiesTable) {
        window.reloadAllergiesTable();
    }
}
