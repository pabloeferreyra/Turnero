(() => {
    const storageKey = 'turnero-color-scheme';
    const root = document.documentElement;
    const colorSchemeMeta = document.querySelector('meta[name="color-scheme"]');
    const systemPreference = window.matchMedia('(prefers-color-scheme: dark)');
    let preference = readPreference();

    function readPreference() {
        try {
            const value = window.localStorage.getItem(storageKey);
            return value === 'dark' || value === 'light' ? value : null;
        } catch (error) {
            console.warn('No se pudo leer la preferencia de tema.', error);
            return null;
        }
    }

    function getSystemTheme() {
        return systemPreference.matches ? 'dark' : 'light';
    }

    function applyTheme() {
        const theme = preference ?? getSystemTheme();
        root.dataset.bsTheme = theme;
        root.style.colorScheme = theme;
        colorSchemeMeta.content = preference ?? 'light dark';

        const toggle = document.getElementById('themeToggle');
        const icon = document.getElementById('themeToggleIcon');
        if (toggle && icon) {
            const nextTheme = theme === 'dark' ? 'light' : 'dark';
            const label = `Activar modo ${nextTheme === 'dark' ? 'oscuro' : 'claro'}`;
            toggle.setAttribute('aria-label', label);
            toggle.title = label;
            icon.className = `bi ${theme === 'dark' ? 'bi-sun' : 'bi-moon-stars'}`;
        }

        document.dispatchEvent(new CustomEvent('turnero:themechange', { detail: { theme } }));
    }

    function savePreference(theme) {
        preference = theme;
        try {
            if (theme) {
                window.localStorage.setItem(storageKey, theme);
            } else {
                window.localStorage.removeItem(storageKey);
            }
        } catch (error) {
            console.warn('No se pudo guardar la preferencia de tema.', error);
        }
        applyTheme();
    }

    applyTheme();

    document.addEventListener('DOMContentLoaded', () => {
        applyTheme();
        document.getElementById('themeToggle')?.addEventListener('click', () => {
            const nextTheme = (preference ?? getSystemTheme()) === 'dark' ? 'light' : 'dark';
            savePreference(nextTheme === getSystemTheme() ? null : nextTheme);
        });
    });

    systemPreference.addEventListener('change', () => {
        if (!preference) {
            applyTheme();
        }
    });

    window.addEventListener('storage', (event) => {
        if (event.key === storageKey || event.key === null) {
            preference = readPreference();
            applyTheme();
        }
    });
})();
