// @ts-nocheck
'use strict';

document.addEventListener("modal:updated", (e) => {
    if (e.detail.modalId !== "GlobalModal") return;

    const btn = document.querySelector("[data-save-background]");
    if (!btn) return;

    btn.addEventListener("click", () => {
        window.SavePersonalBackground();
    }, { once: true });
});

window.SavePersonalBackground = () =>
    ModalUtils.submitForm(
        "GlobalModal",                    // modal destino
        "PersonalBackgroundEditForm",     // ID del form
        "/PersonalBackground/Edit",       // URL
        "PUT",                            // método
        "Antecedentes personales",
        true,
        true
    );
