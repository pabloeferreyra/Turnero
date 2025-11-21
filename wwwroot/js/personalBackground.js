// @ts-nocheck
'use strict';
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
