import React, { type SyntheticEvent, useCallback, useMemo, useState } from "react";
import { type IViewEditToggleContext, ViewEditToggleContext } from "./ViewEditToggleContext";

export interface IViewEditToggleProps {
    readonly initialMode?: "view" | "edit";

    readonly viewComponent: React.ComponentType;
    readonly editComponent: React.ComponentType;
}

export function ViewEditToggle({ initialMode = "view", viewComponent, editComponent }: IViewEditToggleProps): React.JSX.Element {
    const [mode, setMode] = useState(initialMode);

    const setViewMode = useCallback(
        (event?: SyntheticEvent) => {
            event?.preventDefault();
            setMode("view");
        },
        [setMode]
    );
    const setEditMode = useCallback(
        (event?: SyntheticEvent) => {
            event?.preventDefault();
            setMode("edit");
        },
        [setMode]
    );

    const viewEditToggleContext: IViewEditToggleContext = useMemo(
        () => ({
            mode,
            isInViewMode: mode === "view",
            isInEditMode: mode === "edit",

            goToViewMode: setViewMode,
            goToEditMode: setEditMode
        }),
        [mode, setViewMode, setEditMode]
    );

    let Component: React.ComponentType;
    switch (mode) {
        case "view":
            Component = viewComponent;
            break;

        case "edit":
            Component = editComponent;
            break;

        default:
            throw new Error(`Unknown '${mode}' mode`);
    }

    return (
        <ViewEditToggleContext.Provider value={viewEditToggleContext}>
            <Component />
        </ViewEditToggleContext.Provider>
    );
}