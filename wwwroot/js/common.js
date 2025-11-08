'use strict';

window.AppUtils = window.AppUtils || {};
AppUtils.Pagination = AppUtils.Pagination || {};

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
            Swal.fire({
                position: 'top-end',
                icon: 'info',
                title: 'No puede seleccionar fechas anteriores a hoy.',
                showConfirmButton: false,
                timer: 1200
            });
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
            Swal.fire({
                position: 'top-end',
                icon: 'error',
                title: 'Error cargando datos',
                showConfirmButton: false,
                timer: 1200
            });
        }
    });
};

AppUtils.showToast = function (icon, title, ms = 1200) {
    Swal.fire({
        position: 'top-end',
        icon,
        title,
        showConfirmButton: false,
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
    if (s.includes(" ")) {
        s = s.split(" ")[0];
    }
    if (s.includes("-")) {
        return s;
    }
    const [d, m, y] = s.split("/");
    return `${y}-${m}-${d}`;
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

(function () {
    const AU = window.AppUtils;

    // key -> { currentPage, pageSize, recordsTotal, order:{column,dir} }
    AU._tables = AU._tables || Object.create(null);

    function ensureKey(key) {
        if (!AU._tables[key]) {
            AU._tables[key] = {
                currentPage: 1,
                pageSize: 25,
                recordsTotal: 0,
                order: { column: 0, dir: 'asc' }
            };
        }
        return AU._tables[key];
    }

    // ---------------- Pagination ----------------
    AU.Pagination = {
        init(key, opts = {}) {
            const st = ensureKey(key);
            if (opts.defaultPageSize != null) st.pageSize = parseInt(opts.defaultPageSize, 10) || st.pageSize;
            if (opts.defaultOrder) {
                st.order = {
                    column: Number(opts.defaultOrder.column) || 0,
                    dir: (opts.defaultOrder.dir || 'asc').toLowerCase() === 'desc' ? 'desc' : 'asc'
                };
            }
            if (opts.pageSizeSelector) {
                const sel = opts.pageSizeSelector;
                $(document).off(`change.pageSize.${key}`, sel).on(`change.pageSize.${key}`, sel, () => {
                    const v = parseInt($(sel).val(), 10);
                    st.pageSize = isNaN(v) ? st.pageSize : v;
                    st.currentPage = 1;
                    if (typeof opts.onChange === 'function') opts.onChange();
                });
            }
        },

        // Nuevo: order vive acá
        setOrder(key, column, dir, onChange) {
            const st = ensureKey(key);
            st.order = { column: Number(column) || 0, dir: (dir || 'asc').toLowerCase() === 'desc' ? 'desc' : 'asc' };
            st.currentPage = 1;
            if (typeof onChange === 'function') onChange();
        },

        getOrder(key) {
            return ensureKey(key).order;
        },

        // Back-compat helpers
        setRecordsTotal(key, n) { ensureKey(key).recordsTotal = Number(n) || 0; },
        getState(key) { return ensureKey(key); },

        goTo(key, page, onChange) {
            const st = ensureKey(key);
            st.currentPage = Math.max(1, Number(page) || 1);
            if (typeof onChange === 'function') onChange();
        },
        next(key, onChange) {
            const st = ensureKey(key);
            if (st.currentPage * st.pageSize < st.recordsTotal) {
                st.currentPage++;
                if (typeof onChange === 'function') onChange();
            }
        },
        prev(key, onChange) {
            const st = ensureKey(key);
            if (st.currentPage > 1) {
                st.currentPage--;
                if (typeof onChange === 'function') onChange();
            }
        },

        // Render de paginación leyendo el state unificado
        renderWithState(paginationSelector, key, reloadFn) {
            const st = ensureKey(key);
            const $p = $(paginationSelector).empty();
            if (st.pageSize <= 0 || st.pageSize === -1) return;

            const totalPages = Math.max(1, Math.ceil(st.recordsTotal / st.pageSize));
            const startPage = Math.max(1, st.currentPage - 2);
            const endPage = Math.min(totalPages, st.currentPage + 2);

            function li(p, text, active) {
                const $li = $('<li>').addClass('page-item' + (active ? ' active' : ''));
                const $a = $('<a href="#" class="page-link">').text(text);
                $a.on('click', (e) => {
                    e.preventDefault();
                    AU.Pagination.goTo(key, p, reloadFn);
                });
                $li.append($a);
                return $li;
            }

            if (st.currentPage > 1) $p.append(li(st.currentPage - 1, 'Anterior', false));
            for (let i = startPage; i <= endPage; i++) $p.append(li(i, i, i === st.currentPage));
            if (st.currentPage < totalPages) $p.append(li(st.currentPage + 1, 'Siguiente', false));
        }
    };

    // ---------------- Sort ----------------
    AU.Sort = {
        // headerSelector: botones con data-index, ej: '#visits thead .sort-btn'
        attachHeaderSorting(tableSelector, key, reloadFn) {
            const headerSelector = `${tableSelector} thead .sort-btn`;
            $(document).off(`click.sort.${key}`, headerSelector).on(`click.sort.${key}`, headerSelector, function (e) {
                e.preventDefault();
                const idx = Number($(this).data('index')) || 0;
                const cur = AU.Pagination.getOrder(key);
                const dir = (cur.column === idx && cur.dir === 'asc') ? 'desc' : 'asc';
                AU.Pagination.setOrder(key, idx, dir, reloadFn);
                // Visual
                const $btns = $(headerSelector);
                $btns.removeClass('active').attr('aria-sort', 'none').find('.sort-indicator').text('');
                $(this).addClass('active').attr('aria-sort', dir === 'asc' ? 'ascending' : 'descending')
                    .find('.sort-indicator').text(dir === 'asc' ? '▲' : '▼');
            });
        },

        // Mantengo getOrder por compatibilidad, ahora lee desde Pagination
        getOrder(key) {
            return AU.Pagination.getOrder(key);
        }
    };
})();
