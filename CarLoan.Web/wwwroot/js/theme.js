// Light or dark. A choice made with the header toggle is remembered in this browser; with no
// choice the system setting decides, and the page follows it if it changes. Choosing the theme
// the system gives forgets the choice, so the toggle is also the way back to following the
// system. Loaded in <head> without defer, so the theme is set before the first paint.
(() => {
    const storageKey = 'carloan-theme';
    const root = document.documentElement;
    const systemDark = window.matchMedia('(prefers-color-scheme: dark)');

    const savedTheme = () => {
        try {
            return localStorage.getItem(storageKey);
        } catch {
            return null;
        }
    };

    const syncToggles = () => {
        document.querySelectorAll('[data-theme-toggle]').forEach(toggle =>
            toggle.setAttribute('aria-pressed', String(root.dataset.theme === 'dark')));
    };

    const systemTheme = () => (systemDark.matches ? 'dark' : 'light');

    const apply = theme => {
        root.dataset.theme = theme;
        syncToggles();
    };

    const applyCurrent = () => apply(savedTheme() ?? systemTheme());

    applyCurrent();

    systemDark.addEventListener('change', event => {
        if (savedTheme() === null) {
            apply(event.matches ? 'dark' : 'light');
        }
    });

    // Blazor's enhanced navigation merges in server HTML that has no theme and an unpressed
    // toggle, so the current theme is applied again after it.
    document.addEventListener('DOMContentLoaded', () => {
        syncToggles();
        window.Blazor?.addEventListener?.('enhancedload', applyCurrent);
    });

    document.addEventListener('click', event => {
        if (!event.target.closest('[data-theme-toggle]')) {
            return;
        }

        const next = root.dataset.theme === 'dark' ? 'light' : 'dark';
        try {
            if (next === systemTheme()) {
                localStorage.removeItem(storageKey);
            } else {
                localStorage.setItem(storageKey, next);
            }
        } catch {
            // Storage blocked: the choice still applies for this visit.
        }
        apply(next);
    });
})();
