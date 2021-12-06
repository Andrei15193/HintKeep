import type { PropsWithChildren } from "react";
import React from "react";

export function Then({ children }: PropsWithChildren<{}>): JSX.Element {
    return <>{children}</>;
}