// @ts-nocheck
'use strict';

window.PersonalBackgroundView = async function (id) {
    const response = await fetch(`/PersonalBackground/Index?id=${id}`);
    const html = await response.text();

    document.getElementById("PersonalBackgroundContent").innerHTML = html;
    setTimeout(() => {
        const modalEl = document.getElementById('PersonalBackgroundModal');
        const modal = new bootstrap.Modal(modalEl);
        modal.show();
    }, 10);
};

window.EditPersonalBackground = function (patientId) {

    $.ajax({
        url: '/PersonalBackground/Edit',
        type: 'GET',
        data: { id: patientId },

        success: function (response) {

            $('#PersonalBackgroundContent').html(response);

            setTimeout(() => {
                const modalEl = document.getElementById('PersonalBackgroundModal');
                const modal = new bootstrap.Modal(modalEl);
                modal.show();
            }, 10);
        }
    });
};

window.SavePersonalBackground = async function () {

    const form = document.getElementById("PersonalBackgroundEditForm");
    const formData = new FormData(form);

    const checkboxes = form.querySelectorAll('input[type="checkbox"][name]');

    checkboxes.forEach(cb => {
        formData.delete(cb.name);
        formData.append(cb.name, cb.checked ? "true" : "false");
    });

    try {
        const response = await fetch('/PersonalBackground/Edit', {
            method: 'PUT', 
            body: formData
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(error || "No se pudo guardar");
        }

        const html = await response.text();
        document.getElementById("PersonalBackgroundContent").innerHTML = html;

        if (window.Swal) {
            Swal.fire({
                title: "Guardado",
                text: "Los antecedentes fueron actualizados.",
                icon: "success",
                timer: 900,
                showConfirmButton: false
            });
        }
    } catch (err) {
        if (window.Swal) {
            Swal.fire("Error", err.message, "error");
        } else {
            alert(err.message);
        }
    }
};

window.ClosePersonalBackground = function () {
    forceCloseModal();
};

function forceCloseModal() {
    const modalEl = document.getElementById("PersonalBackgroundModal");
    const modal = bootstrap.Modal.getInstance(modalEl)
        || new bootstrap.Modal(modalEl);

    modal.hide();

    document.body.classList.remove("modal-open");
    document.body.style.removeProperty("overflow");

    const backdrops = document.querySelectorAll(".modal-backdrop");
    backdrops.forEach(b => b.remove());
}