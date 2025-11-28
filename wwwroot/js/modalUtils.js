// @ts-nocheck
'use strict';

window.ModalUtils = {
    load(modalId, url, title = null) {
        const modalEl = document.getElementById(modalId);
        if (!modalEl) {
            console.error("Modal no encontrado:", modalId);
            return;
        }

        fetch(url)
            .then(r => r.text())
            .then(html => {
                this.update(modalId, { html, title });
                this.open(modalId);
            })
            .catch(err => {
                console.error("Error cargando modal:", err);

                if (window.Swal) AppUtils.showToast('error', 'No se puede cargar el modal');
            });
    },

    getModal(modalId) {
        const el = document.getElementById(modalId);
        if (!el) {
            console.error("Modal no encontrado:", modalId);
            return null;
        }

        // Reutilizar instancia si ya está creada
        let instance = bootstrap.Modal.getInstance(el);

        if (!instance) {
            instance = new bootstrap.Modal(el, { backdrop: "static" });
        }

        return instance;
    },

    open(modalId) {
        const modal = this.getModal(modalId);
        if (modal) modal.show();
    },

    close(modalId) {
        const modal = this.getModal(modalId);
        if (modal) modal.hide();
    },

    update(modalId, { html = null, title = null }) {

        const body = document.getElementById(`${modalId}-body`);
        const t = document.getElementById(`${modalId}-title`);

        if (!body) return console.error(`Modal body no encontrado: ${modalId}-body`);

        if (html !== null) body.innerHTML = html;
        if (title !== null && t) t.textContent = title;
        document.dispatchEvent(
            new CustomEvent("modal:updated", { detail: { modalId } })
        );
    },

    async submitForm(modalId, formId, url, method = "POST", title = null, showSuccess = true, update = false) {

        const form = document.getElementById(formId);
        
        if (!form) throw new Error("Formulario no encontrado: " + formId);

        const formData = new FormData(form);

        const checkboxes = form.querySelectorAll('input[type="checkbox"][name]');
        checkboxes.forEach(cb => {
            formData.delete(cb.name);
            formData.append(cb.name, cb.checked ? "true" : "false");
        });

        try {
            var response = null;
            if(method === "PUT"){
                response = await fetch(url, { method, body: formData });
            } else if (method === "POST") {
                const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
                response = await fetch(url, { method, headers: {"RequestVerificationToken": token}, body: formData });
            }
            if (!response.ok) {
                error = await response.text();
                throw new Error(error || "Error en el guardado");
            }

            const html = await response.text();

            if (update) {
                this.update(modalId, { html, title });
            }

            if (showSuccess && window.Swal) {
                AppUtils.showToast('success', 'Guardado correctamente', false, 900);
            }

            if (!update) {
                this.close(modalId);
            }

        } catch (err) {
            if (window.Swal) {
                AppUtils.showToast('error', err.message, true);
            } else {
                alert(err.message);
            }
        }
    }
};
