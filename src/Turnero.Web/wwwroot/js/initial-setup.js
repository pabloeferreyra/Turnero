(() => {
    const list = document.querySelector("#user-list");
    const template = document.querySelector("#user-entry-template");
    const addButton = document.querySelector("#add-user");
    const status = document.querySelector("#user-list-status");

    if (!list || !template || !addButton) {
        return;
    }

    const reindex = () => {
        const entries = [...list.querySelectorAll("[data-user-entry]")];
        entries.forEach((entry, index) => {
            entry.querySelector("[data-user-heading]").textContent = `Usuario ${index + 1}`;
            const ids = new Map();
            entry.querySelectorAll("[name]").forEach((control) => {
                const field = control.name.split(".").at(-1);
                const id = `setup-user-${index}-${field.toLowerCase()}`;
                ids.set(control.id, id);
                control.name = `Users[${index}].${field}`;
                control.id = id;
            });
            entry.querySelectorAll("label[for]").forEach((label) => {
                label.htmlFor = ids.get(label.htmlFor);
            });
            entry.querySelector("[data-remove-user]").disabled = entries.length <= 1;
        });

        addButton.disabled = entries.length >= 25;
        status.textContent = entries.length >= 25 ? "Puede agregar hasta 25 usuarios por vez." : "";
    };

    addButton.addEventListener("click", () => {
        if (list.querySelectorAll("[data-user-entry]").length >= 25) {
            return;
        }

        list.append(template.content.cloneNode(true));
        reindex();
        list.querySelector("[data-user-entry]:last-child input")?.focus();
    });

    list.addEventListener("click", (event) => {
        const removeButton = event.target.closest("[data-remove-user]");
        if (!removeButton || list.querySelectorAll("[data-user-entry]").length <= 1) {
            return;
        }

        removeButton.closest("[data-user-entry]").remove();
        reindex();
    });

    reindex();
})();
