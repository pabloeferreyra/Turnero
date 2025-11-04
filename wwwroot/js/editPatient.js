document.addEventListener("patients:editLoaded", function () {

    AppUtils.initFlatpickr("#BirthDateEdit", { maxToday: true });

    $(document).on("input", "#PhoneEdit, #DniEdit, #PostalCodeEdit, #AffiliateNumberEdit", function () {
        this.value = this.value.replace(/\D+/g, "");
    });

    AppUtils.FormValidationRules("#btnEditarPaciente", {
        Name: { required: true },
        PhoneEdit: { minLength: 6 },
        BirthDateEdit: { required: true }
    });

    $(document).on("click", "#btnEditarPaciente", async function (e) {
        e.preventDefault();

        if (!AppUtils.validateAll()) return;

        await EditPatientSubmit();
    });
});


async function EditPatientSubmit() {
    const form = document.querySelector("#EditForm");
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const data = new FormData(form);

    try {
        const resp = await fetch("/Patients/Edit", {
            method: "PUT",
            headers: { "RequestVerificationToken": token },
            body: data
        });

        if (!resp.ok) throw new Error();

        $("#Edit").modal("toggle");

        AppUtils.showToast("success", "Paciente editado.", 900);

        if (typeof window.reloadPatientsTable === "function") {
            window.reloadPatientsTable();
        }

    } catch {
        AppUtils.showToast("error", "Error editando paciente");
    }
}
