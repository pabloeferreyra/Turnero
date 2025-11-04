document.addEventListener("turns:editLoaded", function () {

    AppUtils.initFlatpickr("#DateTurnEdit", {
        minToday: true,
        blockSundays: true
    });

    AppUtils.FormValidationRules("#btnEditarTurno", {
        clientName: { required: true },
        dniCliente: { required: true, minLength: 6 },
        DateTurnEdit: { required: true },
        TimeIdEdit: { required: true }
    });

    // permitir solo números DNI
    $(document).on("input", "#DniEdit", function () {
        this.value = this.value.replace(/\D+/g, "");
    });

    // validación especial time > ahora si es hoy
    $(document).on('change', '#TimeIdEdit', function () {
        const $btn = $('#btnEditarTurno');

        const iso = $('#DateTurnEdit').val().trim();
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
            $btn.prop('disabled', true);
            AppUtils.showToast("info", "La hora debe ser posterior al ahora.");
        }
    });

    // blur crossfire → validar 1 solo campo
    $(document).on('blur', '#clientName, #DniEdit, #DateTurnEdit, #TimeIdEdit', function () {
        AppUtils.validateField(`#${this.id}`);
    });

    $(document).on('click', '#btnEditarTurno', async function (ev) {
        ev.preventDefault();
        if (!AppUtils.validateAll()) return;
        await EditTurn();
    });
});


async function EditTurn() {
    const form = document.querySelector("#EditForm");
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const data = new FormData(form);

    const response = await fetch("/Turns/Edit", {
        method: "PUT",
        headers: { "RequestVerificationToken": token },
        body: data
    });

    if (!response.ok) {
        AppUtils.showToast("error", "No se pudo editar el turno.");
        return;
    }

    $("#Edit").modal("toggle");
    reset?.();
    AppUtils.showToast("success", "Turno editado correctamente.", 600);
}