document.addEventListener("patients:editLoaded", () => {

    // si tu form tiene un datepicker, activalo acá
    AppUtils.initFlatpickr("#BirthDateEdit", { maxToday: true, blockSundays: false });

    // botón guardar
    $(document).off("click.patientsEdit", "#btnEditarPaciente")
        .on("click.patientsEdit", "#btnEditarPaciente", async function (e) {
            e.preventDefault();

            const form = document.querySelector("#EditForm");
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            const data = new FormData(form);

            const r = await fetch("/Patients/Edit", {
                method: "PUT",
                headers: { "RequestVerificationToken": token },
                body: data
            });

            if (!r.ok) {
                AppUtils.showToast("error", "No se pudo editar el paciente.");
                return;
            }

            $("#Edit").modal("hide");
            AppUtils.showToast("success", "Paciente editado correctamente.", 600);

            if (window.patients_loadData) window.patients_loadData();
        });
});