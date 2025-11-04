document.addEventListener("patients:createLoaded", function () {
    AppUtils.initFlatpickr("#BirthDate", { maxToday: true });

    if (typeof AppUtils.FormValidationRules !== 'undefined') {
        AppUtils.FormValidationRules("#btnCrearPaciente", {
            Name: { required: true },
            Dni: { required: true, minLength: 6 },
            Phone: { minLength: 6 },
            BirthDate: { required: true }
        });
    }
});

$(document).on("input", "#Dni, #Phone, #PostalCode, #AffiliateNumber", function () {
    this.value = this.value.replace(/\D+/g, "");
});

$(document).on('click', '#btnCrearPaciente', function (e) {
    e.preventDefault();
    Create();
});


function Create() {

    var form = $('#__AjaxAntiForgeryForm'); 
    var token = $('input[name="__RequestVerificationToken"]', form).val();

    const Patient = {
        Name: $('#Name').val(),
        Dni: $('#Dni').val(),
        BirthDate: $('#BirthDate').val(),
        SocialWork: $('#SocialWork').val(),
        AffiliateNumber: $('#AffiliateNumber').val(),
        ContactInfo: {
            Phone: $('#Phone').val(),
            Email: $('#Email').val(),
            Address: $('#Address').val(),
            City: $('#City').val(),
            PostalCode: $('#PostalCode').val()
        }
    };

    $.ajax({
        type: "POST",
        url: "/Patients/Create",
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify(Patient),
        contentType: "application/json; charset=UTF-8",
        success: function () {

            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: 'Paciente creado con éxito.',
                showConfirmButton: false,
                timer: 900
            }).then(() => {
                $('#Create').modal('toggle');
                if (typeof window.reloadPatientsTable === 'function') {
                    window.reloadPatientsTable();
                }
            });
        },
        error: function () {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Ocurrió un error al procesar la solicitud.'
            });
        }
    });
}
