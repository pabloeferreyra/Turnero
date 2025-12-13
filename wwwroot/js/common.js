// @ts-nocheck
'use strict';

window.AppUtils = window.AppUtils || {};
AppUtils.Pagination = AppUtils.Pagination || {};
AppUtils.Date = AppUtils.Date || {};

AppUtils.todayString = function () {
    const d = new Date();
    const yyyy = d.getFullYear();
    const mm = (d.getMonth() + 1).toString().padStart(2, '0');
    const dd = d.getDate().toString().padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
};

AppUtils.bindDateValidation = function ($input, $btn, options = {}) {
    if (!$input || !$input.length) return;

    const cfg = Object.assign({
        minToday: false,
        blockWeekends: true
    }, options);

    const safeDisable = (state) => {
        if ($btn && $btn.length && typeof $btn.prop === 'function') {
            try { $btn.prop('disabled', state); } catch (e) { console.warn('No se pudo cambiar estado del botón:', e); }
        }
    };

    if (cfg.minToday) $input.attr('min', AppUtils.todayString());
    else $input.removeAttr('min');

    $input.on('change input', function () {
        const val = $(this).val();
        if (!val) return;

        const d = new Date(val + 'T00:00:00');
        const today = new Date(); today.setHours(0, 0, 0, 0);
        const day = d.getDay();

        if (cfg.minToday && d < today) {
            AppUtils.showToast('info', 'No puede seleccionar fechas anteriores a hoy.');
            $(this).val(AppUtils.todayString());
            safeDisable(true);
            return;
        }

        safeDisable(false);
    });
};

AppUtils.paginateTable = function (paginationSelector, recordsTotal, pageSize, currentPage, onPageClick) {
    const $pagination = $(paginationSelector);
    if (!$pagination.length) return;

    $pagination.empty();
    if (!recordsTotal || pageSize <= 0) return;

    const totalPages = Math.max(1, Math.ceil(recordsTotal / pageSize));
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);

    const append = (p, text, active) => {
        const $li = $('<li>').addClass('page-item' + (active ? ' active' : ''));
        const $a = $('<a href="#" class="page-link">').text(text).data('page', p);
        $a.on('click', function (e) {
            e.preventDefault();
            const np = $(this).data('page');
            if (np && np >= 1 && typeof onPageClick === 'function') onPageClick(np);
        });
        $li.append($a);
        $pagination.append($li);
    };

    if (currentPage > 1) append(currentPage - 1, 'Anterior', false);
    for (let p = startPage; p <= endPage; p++) append(p, p, p === currentPage);
    if (currentPage < totalPages) append(currentPage + 1, 'Siguiente', false);
};

AppUtils.reloadTable = function (url, payload, successCallback) {
    if (!url) return console.error('reloadTable: URL no especificada');
    $.ajax({
        type: 'POST',
        url,
        data: payload || {},
        dataType: 'json',
        success: function (res) {
            if (typeof successCallback === 'function') successCallback(res);
        },
        error: function (xhr) {
            console.error('Error recargando tabla:', xhr);
            AppUtils.showToast('error', 'Error cargando datos');
        }
    });
};

AppUtils.showToast = function (icon, title, show = false, ms = 1200) {
    Swal.fire({
        position: 'top-end',
        icon,
        title,
        showConfirmButton: show,
        timer: ms
    });
};

AppUtils.isPastISODate = function (iso) {
    const [Y, M, D] = iso.split("-").map(Number);
    const d = new Date(Y, M - 1, D, 0, 0, 0);

    const now = new Date();
    const t = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0);

    return d < t;
};

AppUtils.validateField = function (selector) {
    if (typeof window.FormValidationRules === 'object' && window.FormValidationRules !== null) {
        const fn = window.FormValidationRules[selector];
        if (!fn) return true;
        const el = document.querySelector(selector);
        if (!el) return true;
        return fn(el, { err: msg => AppUtils.showToast("info", msg) });
    }
    return true;
};

AppUtils.validateAll = function () {
    if (typeof window.FormValidationRules === 'object' && window.FormValidationRules !== null) {
        for (const selector in window.FormValidationRules) {
            if (!AppUtils.validateField(selector)) return false;
        }
        return true;
    }
    return true;
};

AppUtils.Sort = {
    parseDateString(s) {
        if (!s) return null;
        s = s.trim();
        let iso = /^\d{4}-\d{2}-\d{2}/;
        if (iso.test(s)) {
            let d = new Date(s);
            return isNaN(d) ? null : d;
        }
        let dm = /^(\d{1,2})\/(\d{1,2})\/(\d{4})$/;
        let m = s.match(dm);
        if (m) {
            let day = parseInt(m[1], 10);
            let month = parseInt(m[2], 10) - 1;
            let year = parseInt(m[3], 10);
            let d = new Date(year, month, day);
            return isNaN(d) ? null : d;
        }
        let d = new Date(s);
        return isNaN(d) ? null : d;
    },

    normalizeValue(val) {
        if (val === null || val === undefined) return { type: 'string', value: '' };
        val = String(val).trim();
        if (val === '') return { type: 'string', value: '' };

        let num = val.replace(/\s/g, '').replace(/\./g, '').replace(',', '.');
        if (/^-?\d+(\.\d+)?$/.test(num)) {
            let n = parseFloat(num);
            if (!isNaN(n)) return { type: 'number', value: n };
        }

        const d = this.parseDateString(val);
        if (d) return { type: 'date', value: d.getTime() };

        return { type: 'string', value: val.toLowerCase() };
    },

    compareValues(a, b) {
        if (a.type === b.type) {
            if (a.value < b.value) return -1;
            if (a.value > b.value) return 1;
            return 0;
        }
        return String(a.value).localeCompare(String(b.value));
    },

    sortTableByColumn(tableSelector, columnIndex, ascending) {
        let tbody = document.querySelector(tableSelector + ' tbody');
        if (!tbody) return;
        let rows = Array.from(tbody.querySelectorAll('tr'));
        let stabilized = rows.map((row, idx) => {
            let raw = row.children[columnIndex]?.textContent.trim() ?? '';
            let norm = this.normalizeValue(raw);
            return { row: row, norm: norm, idx: idx };
        });

        stabilized.sort((a, b) => {
            let cmp = this.compareValues(a.norm, b.norm);
            if (cmp === 0) return a.idx - b.idx;
            return ascending ? cmp : -cmp;
        });

        let fragment = document.createDocumentFragment();
        stabilized.forEach(item => fragment.appendChild(item.row));
        tbody.appendChild(fragment);
    },

    attachHeaderSorting(tableSelector) {

        var sortState = { index: null, asc: true };

        function headerSortHandler(e) {
            var btn = e.currentTarget;
            let index = parseInt(btn.getAttribute('data-index'), 10);
            if (isNaN(index)) return;

            if (sortState.index === index) {
                sortState.asc = !sortState.asc;
            } else {
                sortState.index = index;
                sortState.asc = true;
            }

            let buttons = document.querySelectorAll(tableSelector + ' thead .sort-btn');
            buttons.forEach(function (b) {
                b.classList.remove('active');
                b.setAttribute('aria-sort', 'none');
                let ind = b.querySelector('.sort-indicator');
                if (ind) ind.textContent = '';
            });

            btn.classList.add('active');
            btn.setAttribute('aria-sort', sortState.asc ? 'ascending' : 'descending');
            let ind = btn.querySelector('.sort-indicator');
            if (!ind) {
                ind = document.createElement('span');
                ind.className = 'sort-indicator';
                btn.appendChild(ind);
            }
            ind.textContent = sortState.asc ? '▲' : '▼';

            AppUtils.Sort.sortTableByColumn(tableSelector, index, sortState.asc);
        }

        // set listeners
        let buttons = document.querySelectorAll(tableSelector + ' thead .sort-btn');
        buttons.forEach(function (btn) {
            btn.removeEventListener('click', headerSortHandler);
            btn.addEventListener('click', headerSortHandler);
        });
    },

    getOrder(tableSelector, columnsMap) {
        // busca el botón con class .sort-btn.active dentro de thead
        const btn = document.querySelector(`${tableSelector} thead .sort-btn.active`);
        if (!btn) return { index: 0, asc: true }; // default

        const idx = parseInt(btn.getAttribute('data-index'), 10);
        const asc = btn.getAttribute('aria-sort') !== 'descending';

        // por si un botón no coincide con el columnsMap real
        const colIndex = isNaN(idx) ? 0 : idx;
        return { index: colIndex, asc };
    }
};

AppUtils.Pagination.init = function (key, opts = {}) {

    AppUtils._tables = AppUtils._tables || {};
    AppUtils._tables[key] = AppUtils._tables[key] || {
        currentPage: 1,
        pageSize: 25,
        recordsTotal: 0,
        order: { column: 0, dir: 'asc' }
    };

    const st = AppUtils._tables[key];

    if (opts.defaultPageSize != null) {
        st.pageSize = parseInt(opts.defaultPageSize, 10) || st.pageSize;
    }

    if (opts.defaultOrder) {
        st.order = {
            column: Number(opts.defaultOrder.column) || 0,
            dir: (opts.defaultOrder.dir || 'asc') === 'desc' ? 'desc' : 'asc'
        };
    }

    if (opts.pageSizeSelector) {
        const sel = opts.pageSizeSelector;
        document.addEventListener("change", (e) => {
            if (!e.target.matches(sel)) return;
            const v = parseInt(e.target.value, 10);
            st.pageSize = isNaN(v) ? st.pageSize : v;
            st.currentPage = 1;
            opts.onChange?.();
        });
    }
};

AppUtils.Pagination.getState = function (key) {
    AppUtils._tables = AppUtils._tables || {};
    return AppUtils._tables[key];
};

AppUtils.Pagination.getOrder = function (key) {
    const st = AppUtils.Pagination.getState(key);
    return st?.order || { column: 0, dir: 'asc' };
};

AppUtils.Pagination.setRecordsTotal = function (key, total) {
    const st = AppUtils.Pagination.getState(key);
    if (st) {
        st.recordsTotal = Number(total) || 0;
    }
};

AppUtils.Pagination.goTo = function (key, page, onChange) {
    const st = AppUtils.Pagination.getState(key);
    if (!st) return;
    st.currentPage = Math.max(1, Number(page) || 1);
    onChange?.();
};

AppUtils.Pagination.renderWithState = function (selector, key, onPageChange) {

    const container = document.querySelector(selector);
    if (!container) return;

    const st = AppUtils.Pagination.getState(key);
    if (!st) return;

    const totalPages = Math.max(1, Math.ceil(st.recordsTotal / st.pageSize));
    const current = st.currentPage;

    const MAX_VISIBLE = 7;
    const half = Math.floor(MAX_VISIBLE / 2);

    container.innerHTML = "";

    const ul = document.createElement("ul");
    ul.className = "pagination justify-content-center";

    function add(label, page, disabled = false, active = false) {
        const li = document.createElement("li");
        li.className = "page-item"
            + (disabled ? " disabled" : "")
            + (active ? " active" : "");

        const a = document.createElement("a");
        a.className = "page-link";
        a.href = "#";
        a.textContent = label;

        if (!disabled && typeof page === "number") {
            a.addEventListener("click", e => {
                e.preventDefault();
                AppUtils.Pagination.goTo(key, page, onPageChange);
            });
        }

        li.appendChild(a);
        ul.appendChild(li);
    }

    // «
    add("«", current - 1, current === 1);

    // Rango visible
    let start = Math.max(1, current - half);
    let end = Math.min(totalPages, current + half);

    // Ajustes para bordes
    if (end - start + 1 < MAX_VISIBLE) {
        if (start === 1) {
            end = Math.min(totalPages, start + MAX_VISIBLE - 1);
        } else if (end === totalPages) {
            start = Math.max(1, end - MAX_VISIBLE + 1);
        }
    }

    // Primera página + elipsis
    if (start > 1) {
        add(1, 1);
        if (start > 2) add("…", null, true);
    }

    // Páginas centrales
    for (let i = start; i <= end; i++) {
        add(i, i, false, i === current);
    }

    // Última página + elipsis
    if (end < totalPages) {
        if (end < totalPages - 1) add("…", null, true);
        add(totalPages, totalPages);
    }

    // »
    add("»", current + 1, current === totalPages);

    container.appendChild(ul);
};


AppUtils.Pagination.attach = function (paginationSelector, loadFunc, getPageFunc, getPageSizeFunc, getRecordsTotalFunc) {

    const $pagination = $(paginationSelector);
    if (!$pagination.length) return;

    $pagination.empty();

    const currentPage = getPageFunc();
    const pageSize = getPageSizeFunc();
    const recordsTotal = getRecordsTotalFunc();

    if (pageSize <= 0) return;

    const totalPages = Math.max(1, Math.ceil(recordsTotal / pageSize));
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);

    function addPage(p, label, active) {
        const li = $('<li>').addClass('page-item' + (active ? ' active' : ''));
        const a = $('<a href="#" class="page-link">').text(label);

        a.on('click', function (e) {
            e.preventDefault();
            if (p >= 1 && p <= totalPages) {
                window.currentPage = p;       // ← este es el punto que necesita tu estandar
                loadFunc();
            }
        });

        li.append(a);
        $pagination.append(li);
    }

    if (currentPage > 1) addPage(currentPage - 1, 'Anterior', false);

    for (let p = startPage; p <= endPage; p++) {
        addPage(p, p, p === currentPage);
    }

    if (currentPage < totalPages) addPage(currentPage + 1, 'Siguiente', false);
};

AppUtils.ddmmyyyy_to_iso = function (s) {
    if (!s) return null;
    if (s.includes("-")) {
        s = s.split("-")[0];
    }
    if (s.includes("-")) {
        return s;
    }
    const [d, m, y] = s.split("/");
    return `${y}-${m}-${d}`;
}

AppUtils.toDisplayDate = function (raw) {
    if (!raw) return "";
    const [y, m, d] = raw.split('-').map(Number);
    return new Date(y, m - 1, d).toLocaleDateString("es-AR");
}

AppUtils.initFlatpickr = function (selector, opts) {

    if (!window.flatpickr) {
        console.warn("flatpickr no está cargado aún");
        return;
    }

    let base = {
        dateFormat: "Y-m-d",
        defaultDate: new Date(),
        locale: "es",
        allowInput: true,
        disableMobile: false,
        altInput: true,
        altFormat: "d/m/Y"
    };

    if (opts && opts.maxToday === true) {
        base.maxDate = new Date();
    }
    if (opts && opts.minToday === true) {
        base.minDate = new Date();
    }
    if (opts && opts.blockSundays === true) {
        base.disable = [
            function (date) { return (date.getDay() === 0); }
        ];
    }
    if (opts && opts.defaultDate === true) {
        base.defaultDate = opts.defaultValue;
    }

    flatpickr(selector, base);
};

AppUtils.FormValidationRules = function (submitSelector, rules) {

    // cache
    const $submit = $(submitSelector);

    function evalRule(val, ruleKey, ruleValue) {

        if (ruleKey === 'required') {
            if (ruleValue && (!val || !String(val).trim())) return false;
        }

        if (ruleKey === 'minLength') {
            if (val.length < ruleValue) return false;
        }

        return true;
    }

    function validateOne(id) {
        const $el = $('#' + id);
        if (!$el.length) return true;

        const val = ($el.val() || '').trim();
        const fieldRules = rules[id];

        for (let rk in fieldRules) {
            if (!evalRule(val, rk, fieldRules[rk])) return false;
        }
        return true;
    }

    function validateAll() {
        let ok = true;
        for (let id in rules) {
            if (!validateOne(id)) ok = false;
        }
        $submit.prop('disabled', !ok);
    }

    // input change live
    Object.keys(rules).forEach(id => {
        $(document).on('input change blur', '#' + id, validateAll);
    });

    // first call
    validateAll();
};

document.addEventListener("click", (e) => {
    const btn = e.target.closest("[data-open-modal]");
    if (!btn) return;

    const modalId = btn.dataset.modalId;
    const url = btn.dataset.url;
    const title = btn.dataset.title || null;

    ModalUtils.load(modalId, url, title);
});

document.addEventListener("click", (e) => {
    const btn = e.target.closest("[data-close-modal]");
    if (!btn) return;
    
    const modalId = "GlobalModal";
    ModalUtils.close(modalId);
});

AppUtils.Export = {

    async post({ url, params, filename }) {
        if (!url) throw new Error("Export.post: url requerida");

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        const form = new URLSearchParams();

        if (token) {
            form.append("__RequestVerificationToken", token);
        }

        const data = (typeof params === "function")
            ? params()
            : (params || {});

        Object.entries(data).forEach(([k, v]) => {
            if (v !== undefined && v !== null && v !== "") {
                form.append(k, v);
            }
        });

        const res = await fetch(url, {
            method: "POST",
            body: form
        });

        if (!res.ok) {
            AppUtils.showToast("error", "Error exportando datos");
            return;
        }

        const blob = await res.blob();
        const blobUrl = URL.createObjectURL(blob);

        const a = document.createElement("a");
        a.href = blobUrl;
        a.download = filename || "export";
        document.body.appendChild(a);
        a.click();
        a.remove();

        URL.revokeObjectURL(blobUrl);
    }
};

AppUtils.Date.ddMMyyyy = function (date = new Date()) {
    const d = String(date.getDate()).padStart(2, '0');
    const m = String(date.getMonth() + 1).padStart(2, '0');
    return `${d}${m}${date.getFullYear()}`;
};
