export type AppTheme = "light" | "dark";

// Check for class names & styling in ./index.scss
export type AppThemeCssClassName = "light-theme" | "dark-theme";

const themePreferenceKey = "preferences/theme";
const systemThemeMediaQuery: MediaQueryList | null = window.matchMedia ? window.matchMedia("(prefers-color-scheme: dark)") : null;
let appTheme: AppTheme | null = localStorage.getItem(themePreferenceKey) as AppTheme || null;

export function initialize(): void {
    refreshAppTheme();
}

export function updateAppTheme(appTheme: AppTheme | null | undefined): void {
    if (appTheme === null || appTheme === undefined)
        localStorage.removeItem(themePreferenceKey);
    else
        localStorage.setItem(themePreferenceKey, appTheme);

    refreshAppTheme();
}

export function clearAppTheme(): void {
    updateAppTheme(null);
}

function refreshAppTheme(): void {
    document.body.className = getCssClass();
}

function getCssClass(): AppThemeCssClassName {
    switch (appTheme) {
        case "light":
            return "light-theme";

        case "dark":
            return "dark-theme";

        default:
            if (systemThemeMediaQuery !== null && systemThemeMediaQuery.matches)
                return "dark-theme";
            else
                return "light-theme";
    }
}

if (systemThemeMediaQuery !== null)
    systemThemeMediaQuery.addEventListener("change", refreshAppTheme);