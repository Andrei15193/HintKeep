import React, { useMemo } from "react";

export interface IInfoIconProps {
    readonly action?: boolean;
    readonly className?: string;
}

export function InfoIcon({ action, className }: IInfoIconProps): React.JSX.Element {
    const classNames = useMemo(
        () => [
            "info-icon",
            action ? "action" : null,
            className
        ]
            .filter((value) => value !== null && value !== undefined && value.length > 0 && !/^\s+$/.test(value))
            .join(" "),
        [action, className]
    );

    return (
        <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 22 22"
            className={classNames}
            height="1rem"
            fill="none"
        >
            <circle
                className="stroke"
                stroke="black"
                stroke-width="1.5"
                cx="11"
                cy="11"
                r="10"
            />
            <path
                className="stroke"
                stroke="black"
                stroke-width="1.5"
                stroke-linecap="round"
                d="M11 17V11"
            />
            <circle
                className="fill"
                fill="black"
                cx="11"
                cy="7"
                r="1"
            />
        </svg>
    );
}