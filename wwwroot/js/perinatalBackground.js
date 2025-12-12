// @ts-nocheck
'use strict';

document.addEventListener("modal:updated", (e) => {
    if (e.detail.modalId !== "GlobalModal") return;

    const btn = document.querySelector("[data-save-perinatal]");
    if (!btn) return;

    btn.addEventListener("click", () => {
        window.SavePerinatalBackground();
    }, { once: true });
});

window.SavePerinatalBackground = () =>
    ModalUtils.submitForm(
        "GlobalModal",                    // modal destino
        "PerinatalBackgroundEditForm",     // ID del form
        "/PerinatalBackground/Edit",       // URL
        "PUT",                            // método
        "Antecedentes perinatales",
        true,
        true
    );