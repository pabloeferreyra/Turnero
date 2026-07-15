// @ts-nocheck
(function () {
	'use strict'; 

	const key = 'permMed';
	let currentData = [];

    const resolvePatientId = AppUtils.resolvePatientId;
    const escapeHtml = AppUtils.escapeHtml;

    async function loadData() {
        const tbody = document.getElementById('permMed-body');
        if (!tbody) return;
        const pid = resolvePatientId();
        if (!pid) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center">No hay medicamentos para mostrar.</td></tr>';
            document.querySelector('#permMed-pagination').innerHTML = '';
            document.querySelector('#pageInfoPermMed').textContent = '';
            return;
        }

        const st = AppUtils.Pagination.getState(key);

        const payload = new URLSearchParams({
            draw: 1,
            start: (st.currentPage - 1) * st.pageSize,
            length: st.pageSize,
            patientId: pid,
            "order[0][column]": st.order.column,
            "order[0][dir]": st.order.dir
        });
        const columnsMap = ['Description'];
        columnsMap.forEach((name, i) =>
            payload.append(`columns[${i}][name]`, name)
        );

        const res = await fetch('/PermMed/Initialize', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: payload.toString()
        });

        const response = await res.json();
        currentData = response?.data || response?.Data || [];
        st.recordsTotal = response?.recordsTotal ||
            response?.recordsFiltered ||
            currentData.length ||
            0;

        renderTable();
        AppUtils.Sort.attachHeaderSorting("#permMed", key, loadData);
        AppUtils.Pagination.renderWithState("#permMed-pagination", key, loadData);
        renderPageInfo();
    }

    function renderTable() {
        const tbody = document.getElementById('permMed-body');
        tbody.innerHTML = '';
        if (!currentData.length) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center">No hay medicamentos para mostrar.</td></tr>';
            return;
        }

        currentData.forEach(item => {
            const id = item.id;
            const description = escapeHtml(item.description);

            tbody.insertAdjacentHTML('beforeend', `
               <tr>
                    <td>${description}</td>
                    <td><button class="btn btn-sm btn-danger me-1 btn-deletepm"
                            data-id="${id}">
                            Eliminar
                        </button>
                    </td>
               </tr>
               `);
        });
    }

    function renderPageInfo() {
        const st = AppUtils.Pagination.getState(key);
        const el = document.querySelector('#pageInfoPermMed');
        if (!el) return;

        const startRecord = st.recordsTotal === 0
            ? 0
            : ((st.currentPage - 1) * st.pageSize) + 1;

        const end = Math.min(st.recordsTotal, st.currentPage * st.pageSize);

        el.textContent = `Mostrando ${startRecord} - ${end} de ${st.recordsTotal} medicamentos.`;
    }

    // DELETE INLINE (using shared AppUtils.ConfirmDelete)
    AppUtils.ConfirmDelete.init({
        deleteBtnSelector: '.btn-deletepm',
        confirmBtnSelector: '.btn-confirm-deletepm',
        cancelBtnSelector: '.btn-cancel-deletepm',
        deleteUrlPrefix: '/PermMed/Delete/',
        successMessage: 'Medicamento eliminado correctamente.',
        errorMessage: 'Error al eliminar el medicamento.',
        loadData
    });

    // Cuando el modal termina de inyectarse en el DOM
    document.addEventListener("modal:updated", () => {
        const html = document.querySelector("#GlobalModal-body")?.innerHTML ?? "";

        if (html.includes("btnCreatePermMed")) {
            initCreate();
        }
    });

    function initCreate() {
        AppUtils.FormValidationRules("#formCreatePermMed", {
            Description: { required: true }
        });

        const btn = document.querySelector("#btnCreatePermMed");

        if (btn) {
            btn.addEventListener("click", async function (e) {
                e.preventDefault();
                if (!AppUtils.validateAll()) return;

                await ModalUtils.submitForm(
                    "GlobalModal",
                    "CreatePermMedForm",
                    "/PermMed/Create",
                    "POST",
                    "Nueva medicacion permanente"
                );

                loadData();
            });
        }
    }

    document.addEventListener('DOMContentLoaded', () => {
        AppUtils.TabLoader.init({
            tabSelector: '#permMedTab',
            tableSelector: '#permMed',
            paginationOpts: { key, defaultPageSize: 25, defaultOrder: { column: 0, dir: 'asc' }, pageSizeSelector: '#pageSizePermMed' },
            loadData
        });
    });

})();