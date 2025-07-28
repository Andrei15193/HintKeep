import React, { type PropsWithChildren } from "react";

export interface IInputContainerProps {
    readonly showError: boolean;
    readonly error: string | null;
}

export function InputContainer({ showError, error, children }: PropsWithChildren<IInputContainerProps>): React.JSX.Element {
    return (
        <div className="input-container">
            {children}
            {
                showError
                    ? (
                        <div className="input-container-error">
                            {error}
                        </div>
                    )
                    : null
            }
        </div>
    );
}