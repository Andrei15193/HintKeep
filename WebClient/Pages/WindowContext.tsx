import { createContext, useContext } from "react";

/** This is only used for `sessionStorage`, if it will no longer be used this can be removed. */
export const WindowContext = createContext<Window | null>(null);

/** This is only used for `sessionStorage`, if it will no longer be used this can be removed. */
export function useWindow(): Window {
    return useContext(WindowContext) ?? window;
}