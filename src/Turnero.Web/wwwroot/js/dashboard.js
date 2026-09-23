// @ts-nocheck
'use strict';

(() => {
    // ─── Chart instances ──────────────────────────────────────
    let turnsPerDayChart = null;
    let turnsPerMedicChart = null;
    let turnsPerMedicPerDayChart = null;
    let accessedPieChart = null;
    let turnsPerTimeSlotChart = null;
    let weeklyTrendChart = null;

    // ─── State (with localStorage persistence) ────────────────
    const LS_WEEKLY_MA = 'dashboard_weekly_ma_period';
    const LS_MONTHLY_MA = 'dashboard_monthly_ma_period';

    let _movingAvgPeriod = loadPersisted(LS_WEEKLY_MA, 3);
    let _monthlyAvgPeriod = loadPersisted(LS_MONTHLY_MA, 2);
    let _weeklyData = null;
    let _monthlyData = null;

    // ─── Generic localStorage helpers ─────────────────────────
    function loadPersisted(key, fallback) {
        try {
            const val = parseInt(localStorage.getItem(key), 10);
            return val > 0 ? val : fallback;
        } catch {
            return fallback;
        }
    }

    function savePersisted(key, value) {
        try { localStorage.setItem(key, value); } catch { /* localStorage no disponible */ }
    }

    function syncButtonGroup(selector, activeValue) {
        document.querySelectorAll(selector).forEach(b => {
            const val = parseInt(b.dataset.period, 10);
            const isActive = val === activeValue;
            b.classList.toggle('btn-primary', isActive);
            b.classList.toggle('active', isActive);
            b.classList.toggle('btn-outline-primary', !isActive);
        });
    }

    function initPeriodButtons(selector, onPeriodChange) {
        document.querySelectorAll(selector).forEach(btn => {
            btn.addEventListener('click', () => {
                const period = parseInt(btn.dataset.period, 10);
                if (!period) return;
                onPeriodChange(period);
            });
        });
    }

    // ─── Color palette (matching site.theme.css) ──────────────
    const CHART_COLORS = [
        '#2c698d', '#bae8e8', '#272643', '#5a8fb0',
        '#96d4d4', '#3a3960', '#4d4c7d', '#a9d1e1',
        '#d4e8f0', '#e6f9f9', '#6b9fbf', '#7ab8c8',
    ];

    // ─── DOM ready ────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', () => {
        initDatePickers();
        loadDashboardData();

        document.getElementById('dashboardFilter').addEventListener('submit', (e) => {
            e.preventDefault();
            loadDashboardData();
        });

        document.getElementById('btnLast7').addEventListener('click', () => setDateRange(7));
        document.getElementById('btnLast30').addEventListener('click', () => setDateRange(30));
        document.getElementById('btnLast90').addEventListener('click', () => setDateRange(90));

        // Moving average period buttons
        initPeriodButtons('.ma-period-btn', (period) => {
            if (period === _movingAvgPeriod) return;
            _movingAvgPeriod = period;
            savePersisted(LS_WEEKLY_MA, period);
            syncButtonGroup('.ma-period-btn', period);
            if (_weeklyData) {
                renderWeeklyTrendChart(_weeklyData, period);
                renderWeeklySummaries(_weeklyData);
            }
        });

        initPeriodButtons('.ma-monthly-btn', (period) => {
            if (period === _monthlyAvgPeriod) return;
            _monthlyAvgPeriod = period;
            savePersisted(LS_MONTHLY_MA, period);
            syncButtonGroup('.ma-monthly-btn', period);
            if (_monthlyData) {
                renderMonthlySummaries(_monthlyData);
            }
        });

        // Sync button active states with persisted values on load
        syncButtonGroup('.ma-period-btn', _movingAvgPeriod);
        syncButtonGroup('.ma-monthly-btn', _monthlyAvgPeriod);

        document.getElementById('btnExportDashboard').addEventListener('click', exportDashboardTable);
    });

    // ─── Date pickers ─────────────────────────────────────────
    function initDatePickers() {
        if (typeof flatpickr === 'undefined') {
            console.warn('flatpickr no está cargado');
            return;
        }

        flatpickr('#startDate', {
            dateFormat: 'Y-m-d',
            locale: 'es',
            allowInput: true,
            maxDate: new Date(),
        });

        flatpickr('#endDate', {
            dateFormat: 'Y-m-d',
            locale: 'es',
            allowInput: true,
            maxDate: new Date(),
        });
    }

    function setDateRange(days) {
        const end = new Date();
        const start = new Date();
        start.setDate(start.getDate() - (days - 1));

        document.getElementById('startDate')._flatpickr?.setDate(start);
        document.getElementById('endDate')._flatpickr?.setDate(end);

        loadDashboardData();
    }

    // ─── Load data ────────────────────────────────────────────
    async function loadDashboardData() {
        const startDate = document.getElementById('startDate').value;
        const endDate = document.getElementById('endDate').value;

        if (!startDate || !endDate) {
            AppUtils.showToast('info', 'Seleccione un rango de fechas');
            return;
        }

        showLoading(true);

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
        const medicFilter = document.getElementById('medicFilter')?.value || '';
        const form = new URLSearchParams();
        form.append('__RequestVerificationToken', token);
        form.append('startDate', startDate);
        form.append('endDate', endDate);
        if (medicFilter) {
            form.append('medicId', medicFilter);
        }

        try {
            const res = await fetch('/Dashboard/GetDashboardData', {
                method: 'POST',
                body: form,
            });

            if (!res.ok) {
                const err = await res.json();
                throw new Error(err.error || 'Error al cargar datos');
            }

            const data = await res.json();
            updateDashboard(data);
        } catch (err) {
            console.error('Error:', err);
            AppUtils.showToast('error', err.message);
        } finally {
            showLoading(false);
        }
    }

    function showLoading(show) {
        const loading = document.getElementById('dashboardLoading');
        const content = document.querySelectorAll('.card, #summaryCards');
        if (show) {
            loading.classList.remove('d-none');
            content.forEach(el => el.style.opacity = '0.3');
        } else {
            loading.classList.add('d-none');
            content.forEach(el => el.style.opacity = '1');
        }
    }

    // ─── Update dashboard ─────────────────────────────────────
    function updateDashboard(data) {
        // Summary cards
        document.getElementById('totalTurns').textContent = data.totalTurns;
        document.getElementById('totalAccessed').textContent = data.totalAccessed;
        document.getElementById('totalPending').textContent = data.totalPending;

        // Busiest medic card
        document.getElementById('busiestMedic').textContent = data.busiestMedic || '-';
        document.getElementById('busiestMedicLabel').textContent =
            data.busiestMedic ? `${data.busiestMedicCount} turnos` : 'Médico + ocupado';

        // Busiest medic detail
        document.getElementById('busiestMedicName').textContent = data.busiestMedic || '-';
        document.getElementById('busiestMedicCountDetail').textContent = data.busiestMedicCount || 0;

        // Busiest / quietest day
        document.getElementById('busiestDayName').textContent = data.busiestDay || '-';
        document.getElementById('busiestDayCount').textContent = data.busiestDayCount || 0;
        document.getElementById('quietestDayName').textContent = data.quietestDay || '-';
        document.getElementById('quietestDayCount').textContent = data.quietestDayCount || 0;

        // Charts
        renderTurnsPerDayChart(data.turnsPerDay);
        renderAccessedPieChart(data.totalAccessed, data.totalPending);
        renderTurnsPerMedicChart(data.turnsPerMedic);
        renderTurnsPerTimeSlotChart(data.turnsPerTimeSlot);
        renderTurnsPerMedicPerDayChart(data.turnsPerMedicPerDay);

        // Weekly / Monthly summaries
        // Week-over-week trend — cache the data for period switching
        _weeklyData = data.weeklySummaries;
        renderWeeklyTrendChart(_weeklyData, _movingAvgPeriod);

        // Weekly / Monthly summaries
        _monthlyData = data.monthlySummaries;
        renderWeeklySummaries(data.weeklySummaries);
        renderMonthlySummaries(data.monthlySummaries);

        // Table
        renderDashboardTable(data);
    }

    // ─── Chart: Turns per day ────────────────────────────────
    function renderTurnsPerDayChart(turnsPerDay) {
        const ctx = document.getElementById('turnsPerDayChart').getContext('2d');
        if (turnsPerDayChart) turnsPerDayChart.destroy();

        const labels = turnsPerDay.map(d => d.displayDate);
        const counts = turnsPerDay.map(d => d.count);
        const maxCount = Math.max(...counts, 1);
        const minCount = Math.min(...counts);

        turnsPerDayChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels,
                datasets: [{
                    label: 'Turnos',
                    data: counts,
                    backgroundColor: counts.map(c =>
                        c === maxCount ? 'rgba(40, 167, 69, 0.7)' :
                        c === minCount ? 'rgba(220, 53, 69, 0.7)' :
                        'rgba(44, 105, 141, 0.6)'
                    ),
                    borderColor: counts.map(c =>
                        c === maxCount ? '#28a745' :
                        c === minCount ? '#dc3545' :
                        '#2c698d'
                    ),
                    borderWidth: 2,
                    borderRadius: 4,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: (ctx) => `${ctx.parsed.y} turnos`,
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { maxRotation: 45, font: { size: 11 } },
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: Math.max(1, Math.ceil(maxCount / 5)),
                            font: { size: 11 },
                        },
                        grid: { color: 'rgba(0,0,0,0.06)' },
                    },
                },
                animation: { duration: 600, easing: 'easeOutQuart' },
            },
        });
    }

    // ─── Chart: Accessed vs Pending (donut) ───────────────────
    function renderAccessedPieChart(accessed, pending) {
        const ctx = document.getElementById('accessedPieChart').getContext('2d');
        if (accessedPieChart) accessedPieChart.destroy();

        if (accessed + pending === 0) {
            accessedPieChart = new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: ['Sin datos'],
                    datasets: [{
                        data: [1],
                        backgroundColor: ['#e3f6f5'],
                        borderWidth: 0,
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                },
            });
            return;
        }

        accessedPieChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: [`Ingresados (${accessed})`, `Pendientes (${pending})`],
                datasets: [{
                    data: [accessed, pending],
                    backgroundColor: ['#28a745', '#ffc107'],
                    borderWidth: 3,
                    borderColor: '#fff',
                    hoverOffset: 8,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '65%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { padding: 12, font: { size: 12 }, usePointStyle: true },
                    },
                    tooltip: {
                        callbacks: {
                            label: (ctx) => ctx.label,
                        },
                    },
                },
                animation: { animateRotate: true, duration: 800 },
            },
        });
    }

    // ─── Chart: Turns per medic (horizontal bar) ──────────────
    function renderTurnsPerMedicChart(turnsPerMedic) {
        const ctx = document.getElementById('turnsPerMedicChart').getContext('2d');
        if (turnsPerMedicChart) turnsPerMedicChart.destroy();

        const labels = turnsPerMedic.map(m => m.medicName);
        const counts = turnsPerMedic.map(m => m.count);
        const maxCount = Math.max(...counts, 1);

        turnsPerMedicChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels,
                datasets: [{
                    label: 'Turnos',
                    data: counts,
                    backgroundColor: counts.map((c, i) =>
                        c === maxCount ? 'rgba(40, 167, 69, 0.8)' : CHART_COLORS[i % CHART_COLORS.length]
                    ),
                    borderWidth: 0,
                    borderRadius: 4,
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: (ctx) => `${ctx.parsed.x} turnos`,
                        },
                    },
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: Math.max(1, Math.ceil(maxCount / 5)),
                            font: { size: 11 },
                        },
                        grid: { color: 'rgba(0,0,0,0.06)' },
                    },
                    y: {
                        grid: { display: false },
                        ticks: { font: { size: 11 } },
                    },
                },
                animation: { duration: 600, easing: 'easeOutQuart' },
            },
        });
    }

    // ─── Chart: Turns per time slot (bar) ──────────────────
    function renderTurnsPerTimeSlotChart(turnsPerTimeSlot) {
        const ctx = document.getElementById('turnsPerTimeSlotChart').getContext('2d');
        if (turnsPerTimeSlotChart) turnsPerTimeSlotChart.destroy();

        const labels = turnsPerTimeSlot.map(t => t.time);
        const counts = turnsPerTimeSlot.map(t => t.count);
        const maxCount = Math.max(...counts, 1);

        turnsPerTimeSlotChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels,
                datasets: [{
                    label: 'Turnos',
                    data: counts,
                    backgroundColor: counts.map((c, i) =>
                        c === maxCount ? 'rgba(40, 167, 69, 0.8)' :
                        `rgba(44, 105, 141, ${0.4 + (c / maxCount) * 0.4})`
                    ),
                    borderColor: counts.map(c =>
                        c === maxCount ? '#28a745' : '#2c698d'
                    ),
                    borderWidth: counts.map(c => c === maxCount ? 2 : 1),
                    borderRadius: 4,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: (ctx) => `${ctx.parsed.y} turnos`,
                        },
                    },
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { font: { size: 11 } },
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: Math.max(1, Math.ceil(maxCount / 5)),
                            font: { size: 11 },
                        },
                        grid: { color: 'rgba(0,0,0,0.06)' },
                    },
                },
                animation: { duration: 600, easing: 'easeOutQuart' },
            },
        });
    }

    // ─── Chart: Turns per medic per day (grouped bar) ────────
    function renderTurnsPerMedicPerDayChart(turnsPerMedicPerDay) {
        const ctx = document.getElementById('turnsPerMedicPerDayChart').getContext('2d');
        if (turnsPerMedicPerDayChart) turnsPerMedicPerDayChart.destroy();

        const dateMap = new Map();
        const medicSet = new Set();

        turnsPerMedicPerDay.forEach(item => {
            if (!dateMap.has(item.date)) {
                dateMap.set(item.date, { displayDate: item.displayDate, medics: {} });
            }
            dateMap.get(item.date).medics[item.medicName] = item.count;
            medicSet.add(item.medicName);
        });

        const dates = Array.from(dateMap.entries()).sort((a, b) => a[0].localeCompare(b[0]));
        const medics = Array.from(medicSet).sort();

        if (dates.length === 0) {
            turnsPerMedicPerDayChart = new Chart(ctx, {
                type: 'bar',
                data: { labels: ['Sin datos'], datasets: [{ label: '', data: [0], backgroundColor: '#e3f6f5' }] },
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } } },
            });
            return;
        }

        const labels = dates.map(([_, v]) => v.displayDate);
        const datasets = medics.map((medic, i) => ({
            label: medic,
            data: dates.map(([_, v]) => v.medics[medic] || 0),
            backgroundColor: CHART_COLORS[i % CHART_COLORS.length],
            borderWidth: 0,
            borderRadius: 3,
        }));

        turnsPerMedicPerDayChart = new Chart(ctx, {
            type: 'bar',
            data: { labels, datasets },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        labels: { usePointStyle: true, padding: 16, font: { size: 11 } },
                    },
                    tooltip: { mode: 'index', intersect: false },
                },
                scales: {
                    x: {
                        stacked: false,
                        grid: { display: false },
                        ticks: { font: { size: 10 }, maxRotation: 45 },
                    },
                    y: {
                        stacked: false,
                        beginAtZero: true,
                        ticks: { stepSize: 1, font: { size: 11 } },
                        grid: { color: 'rgba(0,0,0,0.06)' },
                    },
                },
                animation: { duration: 700, easing: 'easeOutQuart' },
            },
        });
    }

    // ─── Chart: Week-over-week trend (line) ───────────────────
    function renderWeeklyTrendChart(weeks, period) {
        const ctx = document.getElementById('weeklyTrendChart')?.getContext('2d');
        if (!ctx) return;
        if (weeklyTrendChart) weeklyTrendChart.destroy();

        if (!weeks || weeks.length < 2) {
            weeklyTrendChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: ['Sin datos'],
                    datasets: [{ label: 'Turnos', data: [0], borderColor: '#e3f6f5', backgroundColor: '#e3f6f5' }]
                },
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } } },
            });
            return;
        }

        const totals = weeks.map(w => w.total);
        const movingAvg = computeMovingAverage(totals, period);

        weeklyTrendChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: weeks.map(w => w.label),
                datasets: [
                    {
                        label: 'Totales',
                        data: totals,
                        borderColor: '#2c698d',
                        backgroundColor: 'rgba(44, 105, 141, 0.08)',
                        borderWidth: 3,
                        pointBackgroundColor: '#2c698d',
                        pointBorderColor: '#fff',
                        pointBorderWidth: 2,
                        pointRadius: 5,
                        pointHoverRadius: 7,
                        tension: 0.3,
                        fill: true,
                    },
                    {
                        label: 'Ingresados',
                        data: weeks.map(w => w.accessed),
                        borderColor: '#28a745',
                        backgroundColor: 'rgba(40, 167, 69, 0.05)',
                        borderWidth: 2,
                        borderDash: [6, 3],
                        pointBackgroundColor: '#28a745',
                        pointBorderColor: '#fff',
                        pointBorderWidth: 2,
                        pointRadius: 4,
                        pointHoverRadius: 6,
                        tension: 0.3,
                        fill: false,
                    },
                    {
                        label: 'Pendientes',
                        data: weeks.map(w => w.pending),
                        borderColor: '#ffc107',
                        backgroundColor: 'rgba(255, 193, 7, 0.05)',
                        borderWidth: 2,
                        borderDash: [3, 3],
                        pointBackgroundColor: '#ffc107',
                        pointBorderColor: '#fff',
                        pointBorderWidth: 2,
                        pointRadius: 4,
                        pointHoverRadius: 6,
                        tension: 0.3,
                        fill: false,
                    },
                    {
                        label: `Prom. móvil (${period} sem)`,
                        data: movingAvg,
                        borderColor: '#e36c2c',
                        backgroundColor: 'transparent',
                        borderWidth: 3,
                        borderDash: [],
                        pointBackgroundColor: '#e36c2c',
                        pointBorderColor: '#fff',
                        pointBorderWidth: 2,
                        pointRadius: 4,
                        pointHoverRadius: 6,
                        pointStyle: 'rectRot',
                        tension: 0.4,
                        fill: false,
                    },
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false,
                },
                plugins: {
                    legend: {
                        position: 'top',
                        labels: {
                            usePointStyle: true,
                            padding: 20,
                            font: { size: 12 },
                        },
                    },
                    tooltip: {
                        callbacks: {
                            label: (ctx) => `${ctx.dataset.label}: ${ctx.parsed.y} turnos`,
                        },
                    },
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { font: { size: 11 } },
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: Math.max(1, Math.ceil(Math.max(...totals) / 6)),
                            font: { size: 11 },
                        },
                        grid: { color: 'rgba(0,0,0,0.06)' },
                    },
                },
                animation: { duration: 800, easing: 'easeOutQuart' },
            },
        });
    }

    // ─── Weekly summaries ─────────────────────────────────────
    function renderWeeklySummaries(weeks) {
        const tbody = document.getElementById('weeklySummaryBody');
        if (!tbody) return;

        if (!weeks || weeks.length === 0) {
            tbody.innerHTML = '<tr><td colspan="9" class="text-center text-muted">Sin datos</td></tr>';
            return;
        }

        const totals = weeks.map(w => w.total);
        const movingAvgs = computeMovingAverage(totals, _movingAvgPeriod);

        tbody.innerHTML = weeks.map((w, i) => {
            const prev = i > 0 ? weeks[i - 1].total : null;
            const variation = prev != null && prev > 0
                ? Math.round(((w.total - prev) / prev) * 100)
                : null;

            let varHtml = '<span class="text-muted">-</span>';
            if (variation !== null) {
                const isUp = variation > 0;
                const isDown = variation < 0;
                const color = isUp ? '#28a745' : isDown ? '#dc3545' : '#6c757d';
                const arrow = isUp ? '▲' : isDown ? '▼' : '◆';
                varHtml = `<span style="color: ${color}; font-weight: 600; font-size: 0.85em;">${arrow} ${Math.abs(variation)}%</span>`;
            }

            // Moving average indicator
            const ma = movingAvgs[i];
            let maHtml = '<span class="text-muted">-</span>';
            if (ma !== null) {
                const above = w.total > ma;
                const below = w.total < ma;
                const diff = Math.round(((w.total - ma) / ma) * 100);
                const color = above ? '#28a745' : below ? '#dc3545' : '#6c757d';
                const arrow = above ? '↗' : below ? '↘' : '→';
                maHtml = `<span title="${escapeHtml(w.label)}: ${w.total} turnos vs tendencia ${ma}" style="white-space: nowrap;">
                    <span style="color: #e36c2c; font-weight: 600;">${ma}</span>
                    <span style="color: ${color}; font-size: 0.8em; margin-left: 2px;">${arrow} ${Math.abs(diff)}%</span>
                </span>`;
            }

            return `
            <tr>
                <td class="fw-semibold">${escapeHtml(w.label)}</td>
                <td><span class="badge" style="background-color: #2c698d;">${w.total}</span></td>
                <td class="text-center">${varHtml}</td>
                <td>${maHtml}</td>
                <td>${w.accessed > 0 ? `<span class="badge bg-success">${w.accessed}</span>` : '<span class="text-muted">0</span>'}</td>
                <td>${w.pending > 0 ? `<span class="badge bg-warning text-dark">${w.pending}</span>` : '<span class="text-muted">0</span>'}</td>
                <td class="fw-bold" style="color: #272643;">${w.avgPerDay}</td>
                <td>${w.daysInRange}</td>
                <td>${escapeHtml(w.busiestDay)} <span class="badge bg-info text-dark">${w.busiestDayCount}</span></td>
            </tr>`;
        }).join('');
    }

    // ─── Monthly summaries ────────────────────────────────────
    function renderMonthlySummaries(months) {
        const tbody = document.getElementById('monthlySummaryBody');
        if (!tbody) return;

        if (!months || months.length === 0) {
            tbody.innerHTML = '<tr><td colspan="9" class="text-center text-muted">Sin datos</td></tr>';
            return;
        }

        const totals = months.map(m => m.total);
        const movingAvgs = computeMovingAverage(totals, _monthlyAvgPeriod);

        tbody.innerHTML = months.map((m, i) => {
            const prev = i > 0 ? months[i - 1].total : null;
            const variation = prev != null && prev > 0
                ? Math.round(((m.total - prev) / prev) * 100)
                : null;

            let varHtml = '<span class="text-muted">-</span>';
            if (variation !== null) {
                const isUp = variation > 0;
                const isDown = variation < 0;
                const color = isUp ? '#28a745' : isDown ? '#dc3545' : '#6c757d';
                const arrow = isUp ? '▲' : isDown ? '▼' : '◆';
                varHtml = `<span style="color: ${color}; font-weight: 600; font-size: 0.85em;">${arrow} ${Math.abs(variation)}%</span>`;
            }

            // Moving average indicator (2-month)
            const ma = movingAvgs[i];
            let maHtml = '<span class="text-muted">-</span>';
            if (ma !== null) {
                const above = m.total > ma;
                const below = m.total < ma;
                const diff = Math.round(((m.total - ma) / ma) * 100);
                const color = above ? '#28a745' : below ? '#dc3545' : '#6c757d';
                const arrow = above ? '↗' : below ? '↘' : '→';
                maHtml = `<span title="${escapeHtml(m.label)}: ${m.total} turnos vs tendencia ${ma}" style="white-space: nowrap;">
                    <span style="color: #e36c2c; font-weight: 600;">${ma}</span>
                    <span style="color: ${color}; font-size: 0.8em; margin-left: 2px;">${arrow} ${Math.abs(diff)}%</span>
                </span>`;
            }

            return `
            <tr>
                <td class="fw-semibold">${escapeHtml(m.label)}</td>
                <td><span class="badge" style="background-color: #2c698d;">${m.total}</span></td>
                <td class="text-center">${varHtml}</td>
                <td>${maHtml}</td>
                <td>${m.accessed > 0 ? `<span class="badge bg-success">${m.accessed}</span>` : '<span class="text-muted">0</span>'}</td>
                <td>${m.pending > 0 ? `<span class="badge bg-warning text-dark">${m.pending}</span>` : '<span class="text-muted">0</span>'}</td>
                <td class="fw-bold" style="color: #272643;">${m.avgPerDay}</td>
                <td>${m.daysInRange}</td>
                <td>${escapeHtml(m.busiestDay)} <span class="badge bg-info text-dark">${m.busiestDayCount}</span></td>
            </tr>`;
        }).join('');
    }

    // ─── Table ────────────────────────────────────────────────
    function renderDashboardTable(data) {
        const tbody = document.getElementById('dashboardTableBody');
        if (!tbody) return;

        const dayData = data.dayAccessedBreakdown || [];
        const medicDayData = data.turnsPerMedicPerDay || [];

        let html = '';

        if (dayData.length === 0 && medicDayData.length === 0) {
            html = '<tr><td colspan="6" class="text-center text-muted">No hay datos en el rango seleccionado</td></tr>';
        } else if (dayData.length > 0) {
            // Build per-day medic counts for the "medic más cargado" column
            const medicPerDay = {};
            medicDayData.forEach(item => {
                if (!medicPerDay[item.date]) medicPerDay[item.date] = [];
                medicPerDay[item.date].push({ name: item.medicName, count: item.count });
            });

            dayData.forEach(day => {
                const medics = medicPerDay[day.date] || [];
                const busiest = medics.length > 0
                    ? medics.sort((a, b) => b.count - a.count)[0]
                    : null;

                const accessedBadge = day.accessed > 0
                    ? `<span class="badge bg-success">${day.accessed}</span>`
                    : `<span class="text-muted">0</span>`;
                const pendingBadge = day.pending > 0
                    ? `<span class="badge bg-warning text-dark">${day.pending}</span>`
                    : `<span class="text-muted">0</span>`;

                html += `<tr>
                    <td>${escapeHtml(day.displayDate)}</td>
                    <td><span class="badge" style="background-color: #2c698d;">${day.total}</span></td>
                    <td>${accessedBadge}</td>
                    <td>${pendingBadge}</td>
                    <td>${busiest ? escapeHtml(busiest.name) : '-'}</td>
                    <td>${busiest ? `<span class="badge" style="background-color: #bae8e8; color: #272643;">${busiest.count}</span>` : '-'}</td>
                </tr>`;
            });
        }

        tbody.innerHTML = html;
    }

    // ─── Moving average helper ────────────────────────────────
    function computeMovingAverage(data, period) {
        const result = new Array(data.length).fill(null);
        for (let i = 0; i < data.length; i++) {
            const start = Math.max(0, i - period + 1);
            const count = i - start + 1;
            let sum = 0;
            for (let j = start; j <= i; j++) {
                sum += data[j];
            }
            result[i] = Math.round((sum / count) * 10) / 10;
        }
        return result;
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text ?? '';
        return div.innerHTML;
    }

    // ─── Export table to CSV ────────────────────────────────
    function exportDashboardTable() {
        const table = document.getElementById('dashboardTable');
        if (!table) return;

        const rows = table.querySelectorAll('tr');
        const csvLines = [];

        rows.forEach(row => {
            const cols = row.querySelectorAll('th, td');
            const vals = Array.from(cols).map(c => {
                let text = c.textContent.trim().replace(/"/g, '""');
                return `"${text}"`;
            });
            csvLines.push(vals.join(','));
        });

        const csv = csvLines.join('\n');
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);

        const a = document.createElement('a');
        a.href = url;
        a.download = `dashboard_${new Date().toISOString().slice(0, 10)}.csv`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(url);

        AppUtils.showToast('success', 'Datos exportados correctamente');
    }
})();
