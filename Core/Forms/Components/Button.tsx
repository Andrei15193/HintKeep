import React, { type MouseEvent, type PropsWithChildren } from "react";

export interface IButtonProps {
    readonly text?: string;

    onClick(event: MouseEvent<HTMLButtonElement>): void;
}

export function Button({ text, onClick, children = text }: PropsWithChildren<IButtonProps>): React.JSX.Element {
    return (
        <button
            type="button"
            onClick={onClick}
        >
            {children}
        </button>
    );
}