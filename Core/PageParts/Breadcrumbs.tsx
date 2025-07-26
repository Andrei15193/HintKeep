import React, { type PropsWithChildren, Fragment } from "react";

export interface IBreadcrumbsProps {
    readonly items: readonly (React.JSX.Element | string | null | undefined)[];
}

export function Breadcrumbs({ items, children }: PropsWithChildren<IBreadcrumbsProps>): React.JSX.Element {
    return (
        <aside className="breadcrumbs">
            <ul>
                {
                    items
                        .filter((item) => item !== null && item !== undefined && (typeof item !== "string" || item.length !== 0))
                        .map((item, itemIndex) => (
                            <Fragment key={typeof item === "string" ? item : itemIndex}>
                                <li>
                                    {item}
                                </li>
                            </Fragment>
                        ))
                }
            </ul>

            {children}
        </aside>
    );
}