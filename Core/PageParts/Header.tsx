import React, { type PropsWithChildren } from "react";

export function Header({ children }: PropsWithChildren<{}>): React.JSX.Element {
    return (
        <header>
            {
                typeof children === "string"
                    ? <h1>
                        {children}
                    </h1>
                    : children
            }
        </header>
    );
}