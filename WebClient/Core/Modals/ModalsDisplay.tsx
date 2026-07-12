import React, { useContext } from "react";
import { ModalContext } from "./ModalContext";

export interface IModalsDisplayProps {
}

export function ModalsDisplay({}: IModalsDisplayProps): React.JSX.Element | null {
    const { modals } = useContext(ModalContext);

    if (modals.length === 0)
        return null;

    return (
        <aside className="confirmation-prompt">
            {
                modals.map(([key, content]) => (
                    <div
                        key={key}
                        className="confirmation-prompt-content"
                    >
                        {content}
                    </div>
                ))
            }
        </aside>
    );
}