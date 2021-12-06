import type { PropsWithChildren, ReactElement } from "react";
import React, { Children, Fragment } from "react";
import { Else } from "./else";
import { Then } from "./then";

export interface IUnlessProps {
    readonly condition: boolean;
}

export function Unless({ condition, children }: PropsWithChildren<IUnlessProps>): JSX.Element {
    const thenChildren: ReactElement[] = [];
    const elseChildren: ReactElement[] = [];

    Children.forEach(
        children,
        (child) => {
            if (React.isValidElement(child))
                if (child.type === Then)
                    thenChildren.push(child);
                else if (child.type === Else)
                    elseChildren.push(child);
        }
    );

    if (!condition)
        if (thenChildren.length === 0 && elseChildren.length === 0)
            return <>{children}</>;
        else if (thenChildren.length === 1)
            return <>{thenChildren[0]}</>;
        else
            return <>{thenChildren.map((child, index) => <Fragment key={index}>{child}</Fragment>)}</>;

    if (elseChildren.length === 1)
        return <>{elseChildren[0]}</>;
    else
        return <>{elseChildren.map((child, index) => <Fragment key={index}>{child}</Fragment>)}</>;
}