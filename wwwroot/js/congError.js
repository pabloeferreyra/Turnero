// @ts-nocheck
'use strict';

document.addEventListener("modal:updated", (e) => {
	if (e.detail.modalId !== "GlobalModal") return;

	const btn = document.querySelector("[data-save-congerrors]");
	if (!btn) return;

	btn.addEventListener("click", () => {
		window.SaveCongError();
	}, { once: true });
});

window.SaveCongError = () =>
	ModalUtils.submitForm(
		"GLobalModal",
		"CongErrorsEditForm",
		"/CongError/Edit",
		"PUT",
		"Errores congénitos",
		true,
		true
	);