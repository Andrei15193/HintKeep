import type { PropsWithChildren } from "react";
import React from "react";

export function Else({ children }: PropsWithChildren<{}>): JSX.Element {
    return <>{children}</>;
}