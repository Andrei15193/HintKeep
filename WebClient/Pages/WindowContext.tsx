import { createContext, useContext } from "react";

/**
 * This is a workaround for React Testing Library to isolate the JSDom Window per scenario
 * providing a fresh state for each run, additionally allowing them to be run in parallel.
 */
export const WindowContext = createContext<Window>(window);

/**
 * This is a workaround for React Testing Library to isolate the JSDom Window per scenario
 * providing a fresh state for each run, additionally allowing them to be run in parallel.
 */
export function useWindow(): Window {
    return useContext(WindowContext);
}