// @ts-nocheck
document.addEventListener("allergies:createLoaded", function () {

    AppUtils.initFlatpickr("#Begin", { maxToday: true });

    AppUtils.initFlatpickr("#End", { maxToday: true });

    AppUtils.FormValidationRules("#btnEditAllergy", {
        Name: { required: true },
        Begin: { required: true },
        End: {
            required: false,
            custom(value) {
                const begin = document.querySelector("#Begin")?.value;
                if (!begin || !value) return true;
                if (value < begin) return "La fecha de fin no puede ser anterior al inicio.";
                return true;
            }
        }
    });

    $(document).on("click", "#btnCreateAllergy", async function (e) {
        e.preventDefault();
        if (!AppUtils.validateAll()) return;
        await CreateAllergySubmit();
    });
});

document.addEventListener("allergies:editLoaded", function () {

    var begin = document.querySelector("#Begin")?.value;
    begin = AppUtils.ddmmyyyy_to_iso(begin);
    
    var end = document.querySelector("#End")?.value;
    end = AppUtils.ddmmyyyy_to_iso(end);

    AppUtils.initFlatpickr("#Begin", { maxToday: true, defaultDate: true, defaultValue: begin });
    AppUtils.initFlatpickr("#End", { maxToday: true, defaultDate: true, defaultValue: end });

    AppUtils.FormValidationRules("#btnEditAllergy", {
        Name: { required: true },
        Begin: { required: true },
        End: {
            required: false,
            custom(value) {
                if (!begin || !value) return true;
                if (value < begin) return "La fecha de fin no puede ser anterior al inicio.";
                return true;
            }
        }
    });

    $(document).on("click", "#btnEditAllergy", async function (e) {
        e.preventDefault();
        if (!AppUtils.validateAll()) return;
        await EditAllergySubmit();
    });
});

async function CreateAllergySubmit() {

    const form = document.querySelector("#CreateAllergyForm");
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const data = new FormData(form);

    const resp = await fetch("/Allergies/Create", {
        method: "POST",
        headers: { "RequestVerificationToken": token },
        body: data
    });

    if (!resp.ok) {
        AppUtils.showToast("error", "Error creando alergia");
        return;
    }

    $("#CreateAllergy").modal("toggle");
    AppUtils.showToast("success", "Alergia creada correctamente", 900);

    if (window.reloadAllergiesTable) {
        window.reloadAllergiesTable();
    }
}

async function EditAllergySubmit() {

    const form = document.querySelector("#CreateAllergyForm");
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const data = new FormData(form);

    const resp = await fetch("/Allergies/Edit", {
        method: "POST",
        headers: { "RequestVerificationToken": token },
        body: data
    });

    if (!resp.ok) {
        AppUtils.showToast("error", "Error editando alergia");
        return;
    }

    $("#CreateAllergy").modal("toggle");
    AppUtils.showToast("success", "Alergia editada correctamente", 900);

    if (window.reloadAllergiesTable) {
        window.reloadAllergiesTable();
    }
}

window.CreateAllergyView = function (patientId) {
    $.ajax({
        type: "GET",
        url: "/Allergies/Create",
        data: { id: patientId },
        success(html) {
            $("#CreateAllergyFormContent").html(html);
            $("#CreateAllergy").modal("show");
            document.dispatchEvent(new Event("allergies:createLoaded"));
        }
    });
};
