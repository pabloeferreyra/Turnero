document.addEventListener("visits:createLoaded", function () {

    AppUtils.initFlatpickr("#VisitDate", {
        maxToday: true
    });

    AppUtils.FormValidationRules("#btnCreateVisit", {
        VisitDate: { required: true }
    });

    $(document).on('click', '#btnCreateVisit', async function (e) {
        e.preventDefault();
        if (!AppUtils.validateAll()) return;
        await createVisitSubmit();
    });
});


async function createVisitSubmit() {

    const form = document.querySelector("#CreateVisitForm");
    const token = form.querySelector('input[name="__RequestVerificationToken"]').value;

    // serialización simple
    const dataObj = Object.fromEntries(new FormData(form).entries());

    const response = await fetch("/Visits/Create", {
        method: "POST",
        headers: {
            "RequestVerificationToken": token,
            "Content-Type": "application/json"
        },
        body: JSON.stringify(dataObj)
    });

    if (!response.ok) {
        AppUtils.showToast("error", "Error creando visita");
        return;
    }

    $("#CreateVisit").modal("toggle");

    if (window.reloadVisitsTable) window.reloadVisitsTable();

    AppUtils.showToast("success", "Visita creada correctamente.", 600);
}


// loader del popup
window.CreateVisitView = function (patientId) {
    $.ajax({
        type: 'GET',
        url: '/Visits/Create',
        data: { id: patientId },
        success: function (html) {
            $('#CreateVisitFormContent').html(html);
            $('#CreateVisit').modal('show');
            document.dispatchEvent(new Event("visits:createLoaded"));
        },
        error: function () {
            AppUtils.showToast('error', 'Error cargando formulario de visita.');
        }
    });
};