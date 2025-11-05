document.addEventListener("patients:createLoaded", () => {

    $(document).off("click.patientsCreate", "#btnCrearPaciente")
        .on("click.patientsCreate", "#btnCrearPaciente", async function (e) {
            e.preventDefault();

            const form = document.querySelector("#CreateForm");
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            const data = new FormData(form);

            const r = await fetch("/Patients/Create", {
                method: "POST",
                headers: { "RequestVerificationToken": token },
                body: data
            });

            if (!r.ok) {
                AppUtils.showToast("error", "No se pudo crear el paciente.");
                return;
            }

            $("#Create").modal("hide");
            AppUtils.showToast("success", "Paciente creado correctamente.", 600);

            if (window.patients_loadData) window.patients_loadData();
        });
});