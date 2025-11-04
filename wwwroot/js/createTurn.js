document.addEventListener("turns:createLoaded", function () {

    AppUtils.initFlatpickr("#DateTurnCreate", {
        minToday: true,
        blockSundays: true
    });

    AppUtils.FormValidationRules("#btnCrearTurno", {
        Name: { required: true },
        Dni: { required: true, minLength: 6 },
        DateTurnCreate: { required: true },
        timeTurnCreate: { required: true }
    });

    $(document).on("input", "#Dni", function () {
        this.value = this.value.replace(/\D+/g, "");
    });

    $(document).on('change', '#timeTurnCreate', function () {

        const iso = $('#DateTurnCreate').val().trim();
        if (!iso) return;

        const now = new Date();
        const todayISO = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;

        if (iso !== todayISO) return;

        const text = this.options[this.selectedIndex].text.trim();
        if (!text) return;

        const [hh, mm] = text.split(":").map(Number);
        const picked = hh * 60 + mm;
        const nowMinutes = now.getHours() * 60 + now.getMinutes();

        if (picked <= nowMinutes) {
            $('#btnCrearTurno').prop('disabled', true);
            AppUtils.showToast("info", "La hora debe ser posterior al ahora.");
        }
    });

    $(document).on('click', '#btnCrearTurno', async function (ev) {
        ev.preventDefault();
        if (!AppUtils.validateAll()) return;
        await createTurn();
    });
});


async function createTurn() {
    const form = document.querySelector("#CreateForm");
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const data = new FormData(form);

    const response = await fetch("/Turns/Create", {
        method: "POST",
        headers: { "RequestVerificationToken": token },
        body: data
    });

    if (!response.ok) {
        AppUtils.showToast("error", "Error creando turno.");
        return;
    }

    $("#Create").modal("toggle");
    reset?.();
    AppUtils.showToast("success", "Turno creado correctamente.", 600);
}
