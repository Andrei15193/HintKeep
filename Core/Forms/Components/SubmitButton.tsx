import React, { type PropsWithChildren } from "react";

export interface ISubmitButtonProps {
    readonly text?: string;
}

export function SubmitButton({ text, children = text }: PropsWithChildren<ISubmitButtonProps>): React.JSX.Element {
    return (
        <button type="submit">
            {children}
        </button>
    );
}