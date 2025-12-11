// @ts-nocheck
(function () {
	'use strict';

	const key = "growthCharts";
	let currentData = [];

	function resolvePatientId() {
		return document.querySelector('#patientId')?.value
			|| document.querySelector('#PatientId')?.value
			|| '';
	}

	function escapeHtml(s) {
		return s ? String(s)
			.replace(/&/g, "&amp;")
			.replace(/</g, "&lt;")
			.replace(/>/g, "&gt;")
			.replace(/"/g, "&quot;")
			.replace(/'/g, "&#039;") : '';
	}

	async function loadData() {

		const tbody = document.getElementById('growthCharts-body');
		if (!tbody) return;

		const pid = resolvePatientId();
		if (!pid) {
			tbody.innerHTML = '<tr><td colspan="5" class="text-center">No hay gráficos de crecimiento para mostrar.</td></tr>';
			document.querySelector('#growthCharts-pagination').innerHTML = '';
			document.querySelector('#pageInfoGrowthCharts').textContent = '';
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

		const columnsMap = ['Age', 'Weight', 'WPerc', 'Height', 'HPerc', 'HeadCircumference', 'HCPerc', 'Bmi'];
		columnsMap.forEach((name, i) =>
			payload.append(`columns[${i}][name]`, name)
		);

		const res = await fetch('/GrowthChart/Initialize', {
			method: 'POST',
			headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
			body: payload
		});

		const response = await res.json();
		currentData = response?.data || response?.Data || [];
		st.recordsTotal =
			response?.recordsTotal ||
			response?.recordsFiltered ||
			currentData.length ||
			0;

		renderTable();

		AppUtils.Sort.attachHeaderSorting("#growthCharts", key, loadData);
		AppUtils.Pagination.renderWithState("#growthCharts-pagination", key, loadData);
		renderPageInfo();
	} 

	function renderTable() {
		const tbody = document.getElementById('growthCharts-body');
		tbody.innerHTML = '';
		if (!currentData.length) {
			tbody.innerHTML = '<tr><td colspan="9" class="text-center">No hay gráficos de crecimiento para mostrar.</td></tr>';
			return;
		}

		currentData.forEach(item => {
			const id = item.id;
			const age = escapeHtml(item.Age + " " + item.Time);
			const weight = escapeHtml(item.Weight);
			const wPerc = escapeHtml(item.WPerc);
			const height = escapeHtml(item.Height);
			const hPerc = escapeHtml(item.HPerc);
			const headCircumference = escapeHtml(item.HeadCircumference);
			const hcPerc = escapeHtml(item.HCPerc);
			const bmi = escapeHtml(item.Bmi);

			tbody.insertAdjacentHTML('beforeend', `
			<tr>
				<td>${age}</td>
				<td>${weight}</td>
				<td>${wPerc}</td>
				<td>${height}</td>
				<td>${hPerc}</td>
				<td>${headCircumference}</td>
				<td>${hcPerc}</td>
				<td>${bmi}</td>
				<td><button class="btn btn-sm btn-danger me-1 btn-deletegc"
						data-id="${id}">
							<i class="fa-solid fa-trash"></i> Eliminar
					</button>
				</td>
			</tr>
			`);
		});
	}

	function renderPageInfo() {
		const st = AppUtils.Pagination.getState(key);
		const el = document.querySelector('#pageInfoGrowthCharts');
		if (!el) return;

		const start = st.recordsTotal === 0
			? 0
			: ((st.currentPage - 1) * st.pageSize) + 1;

		const end = Math.min(st.recordsTotal, st.currentPage * st.pageSize);

		el.textContent = `Mostrando ${start} a ${end} de ${st.recordsTotal} gráficos de crecimiento.`;
	}

	document.addEventListener("DOMContentLoaded", () => {
		console.log('It Works!');
		AppUtils.Pagination.init(key, {
			defaultPageSize: 25,
			defaultOrder: { column: 0, dir: 'asc' },
			pageSizeSelector: '#pageSizeGrowthCharts',
			onChange: loadData
		});

		const tab = document.querySelector('#growthChartsTab');

		if (tab) {
			tab.addEventListener('click', () => {
				
				document.querySelectorAll('#myTabs .nav-link')
					.forEach(x => x.classList.remove('active'));

				tab.classList.add('active');

				const url = tab.dataset.url;
				if (!url) return;

				const container = document.querySelector('#tabContent');

				if (!document.querySelector('#growthCharts')) {
					fetch(url)
						.then(r => r.text())
						.then(html => {
							container.innerHTML = html;
							loadData();
						});
				} else {
					loadData();
				}
			});
		}


		loadData();

	})

	document.addEventListener("modal:updated", () => {
		const html = document.querySelector("#GlobalModal-body")?.innerHTML ?? "";

		if (html.includes("btnCreateGrowthChart")) {
			initCreate();
		}

		if (html.includes("btnEditGrowthChart")) {
			initEdit();
		}
	});

	document.addEventListener('click', (ev) => {
		const target = ev.target;
		if (target.classList.contains('btn-deletegc')) {
			const id = target.dataset.id;
			const cell = target.closest('td');

			target.classList.add('btn-fade-out');
			setTimeout(() => {
				cell.innerHTML = `
				<button data-id="${id}" class="btn btn-danger btn-sm me-1 btn-confirm-deletegc">Si</button>
				<button class="btn btn-secondary btn-sm btn-cancel-deletegc">No</button>
				`;
			}, 200);
		}
	})

	function initCreate() {
		const w = document.getElementById('WeightInKg');
		const h = document.getElementById('HeightInCm');

		if (w) w.addEventListener('input', calculateBmi);
		if (h) h.addEventListener('input', calculateBmi);

		const btn = document.querySelector("#btnCreateGrowthChart");
		if (btn) {
			btn.addEventListener("click", async function (e) {
				e.preventDefault();
				if (!AppUtils.validateAll()) return;

				await ModalUtils.submitForm(
					"GlobalModal",
					"CreateGrowthChartForm",
					"/GrowthChart/Create",
					"POST",
					"Nuevo registro"
				);

				loadData();
			});
		}
	}

	function calculateBmi() {
		const weight = parseFloat(document.getElementById('WeightInKg')?.value) || 0;
		const heightCm = parseFloat(document.getElementById('HeightInCm')?.value) || 0;

		if (weight <= 0 || heightCm <= 0) {
			document.getElementById('Bmi').value = '';
			return;
		}

		const heightM = heightCm / 100;
		const bmi = weight / (heightM * heightM);

		document.getElementById('Bmi').value = bmi.toFixed(2);
	}
})();