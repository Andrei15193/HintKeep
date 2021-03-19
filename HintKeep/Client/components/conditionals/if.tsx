import type { ReactElement, PropsWithChildren } from 'react';
import React, { Children } from 'react';

export interface IIfProps {
    readonly condition: boolean;
}

export function If({ condition, children }: PropsWithChildren<IIfProps>): JSX.Element {
    const matchingChildren: ReactElement[] = [];

    Children.forEach(
        children,
        child => {
            if (React.isValidElement(child) && ((condition && child.type === Then) || (!condition && child.type === Else)))
                matchingChildren.push(child);
        });
    return <>{matchingChildren}</>;
}

export function Then({ children }: PropsWithChildren<{}>): JSX.Element {
    return <>{children}</>;
}

export function Else({ children }: PropsWithChildren<{}>): JSX.Element {
    return <>{children}</>;
}