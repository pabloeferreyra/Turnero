(() => {
    "use strict";

    function initAvailability() {
        const form = document.querySelector("#AvailabilityForm");
        const button = document.querySelector("#btnAvailability");
        if (!form || !button || form.dataset.initialized === "true") return;

        form.dataset.initialized = "true";

        const dateFrom = document.querySelector("#availabilityDateFrom");
        const dateTo = document.querySelector("#availabilityDateTo");
        AppUtils.initFlatpickr(dateFrom, {
            minToday: true,
            disableMobile: true,
            defaultDate: dateFrom?.value || new Date()
        });
        AppUtils.initFlatpickr(dateTo, {
            minToday: true,
            defaultDate: dateTo?.value || new Date()
        });

        const fromPicker = dateFrom?._flatpickr;
        const toPicker = dateTo?._flatpickr;
        if (!fromPicker || !toPicker) return;

        const syncEndDate = () => {
            const selectedFrom = fromPicker.selectedDates[0];
            if (!selectedFrom) return;

            const selectedTo = toPicker.selectedDates[0];
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            const minimum = selectedFrom < today ? today : selectedFrom;
            toPicker.set("minDate", minimum);
            if (selectedTo && selectedTo < minimum) {
                toPicker.setDate(minimum, true);
            }
        };

        fromPicker.set("onChange", [...fromPicker.config.onChange, syncEndDate]);
        syncEndDate();

        const updateUnavailableRow = checkbox => {
            const row = checkbox.closest("[data-availability-day]");
            row?.querySelectorAll(".availability-time").forEach(select => {
                select.disabled = checkbox.checked;
                select.required = !checkbox.checked;
            });
        };

        document.querySelectorAll(".availability-unavailable").forEach(checkbox => {
            checkbox.addEventListener("change", () => updateUnavailableRow(checkbox));
            updateUnavailableRow(checkbox);
        });

        const resetAvailability = () => {
            const today = new Date();
            fromPicker.set("disable", []);
            fromPicker.set("minDate", today);
            fromPicker.setDate(today, true);
            toPicker.setDate(today, true);

            document.querySelectorAll("[data-availability-day]").forEach(row => {
                row.querySelector(".availability-id").value = "";
                row.querySelectorAll(".availability-time").forEach(select => {
                    select.value = "";
                });

                const checkbox = row.querySelector(".availability-unavailable");
                checkbox.checked = row.dataset.availabilityDay === "0";
                updateUnavailableRow(checkbox);
            });
        };

        document.querySelector("#availabilityMedicId")?.addEventListener("change", async event => {
            const medicId = event.target.value;
            if (!medicId) return;

            resetAvailability();
            try {
                const response = await fetch(`/Turns/Availability?medicId=${encodeURIComponent(medicId)}`);
                if (!response.ok) {
                    throw new Error("No se pudo cargar la disponibilidad.");
                }

                const availability = await response.json();
                if (!availability.hasData) return;

                const historicalDate = fromPicker.parseDate(availability.dateFrom, "Y-m-d");
                const today = new Date();
                today.setHours(0, 0, 0, 0);
                if (historicalDate && historicalDate < today) {
                    fromPicker.set("minDate", historicalDate);
                    fromPicker.set("disable", [
                        date => date < today && date.getTime() !== historicalDate.getTime()
                    ]);
                }
                fromPicker.setDate(availability.dateFrom, true);
                toPicker.setDate(availability.dateTo, true);
                syncEndDate();

                availability.days.forEach(day => {
                    const row = document.querySelector(`[data-availability-day="${day.day}"]`);
                    if (!row) return;

                    row.querySelector(".availability-id").value = day.availabilityId ?? "";
                    const selects = row.querySelectorAll(".availability-time");
                    selects[0].value = day.timeFromId ?? "";
                    selects[1].value = day.timeToId ?? "";

                    const checkbox = row.querySelector(".availability-unavailable");
                    checkbox.checked = day.isUnavailable;
                    updateUnavailableRow(checkbox);
                });
            } catch (error) {
                AppUtils.showToast?.("error", error.message, true);
            }
        });

        button.addEventListener("click", async event => {
            event.preventDefault();
            if (!form.reportValidity()) return;

            await ModalUtils.submitForm(
                "GlobalModal",
                "AvailabilityForm",
                "/Turns/Availability",
                "POST",
                "Disponibilidad",
                true,
                false
            );
        });
    }

    document.addEventListener("modal:updated", event => {
        if (event.detail?.modalId === "GlobalModal") {
            initAvailability();
        }
    });
})();
