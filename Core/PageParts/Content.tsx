import React, { type PropsWithChildren } from "react";

export function Content({ children }: PropsWithChildren<{}>): React.JSX.Element {
    return (
        <main className="content">
            {children}
        </main>
    );
}