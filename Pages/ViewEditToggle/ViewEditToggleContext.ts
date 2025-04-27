import { createContext, type SyntheticEvent, useContext } from "react";

export interface IViewEditToggleContext {
    readonly mode: "view" | "edit";

    readonly isInViewMode: boolean;
    readonly isInEditMode: boolean;

    goToViewMode(event?: SyntheticEvent): void;
    goToEditMode(event?: SyntheticEvent): void;
}

export const ViewEditToggleContext = createContext<IViewEditToggleContext | null>(null);

export function useViewEditToggleContext(): IViewEditToggleContext {
    const context = useContext(ViewEditToggleContext);
    if (context === null || context === undefined)
        throw new Error("ViewEditToggleContext has not been set.");

    return context;
}