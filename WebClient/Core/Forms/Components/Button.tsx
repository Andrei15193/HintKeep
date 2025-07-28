import React, { useCallback, useMemo, type MouseEvent, type PropsWithChildren } from "react";
import { Loader } from "../../Loader";

export interface IButtonProps {
    readonly text?: string;
    readonly type?: "submit" | "reset" | "button";

    readonly neutral?: boolean;
    readonly danger?: boolean;
    readonly processing?: boolean;
    readonly disabled?: boolean;

    onClick?(event: MouseEvent<HTMLButtonElement>): void;
}

export function Button({ type = "submit", text, neutral, danger, processing, disabled, children = text, onClick }: PropsWithChildren<IButtonProps>): React.JSX.Element {
    const onClickCallback = useCallback(
        (event: MouseEvent<HTMLButtonElement>): void => {
            if (processing)
                event.preventDefault();
            else
                onClick && onClick(event);
        },
        [processing, onClick]
    );

    const className = useMemo(
        () => [
            neutral && "neutral",
            danger && "danger",
            processing && "processing"
        ]
            .filter((value) => typeof value === "string")
            .join(" "),
        [neutral, danger, processing]
    );

    return (
        <button
            type={type}
            className={className}
            disabled={disabled}
            onClick={onClickCallback}
        >
            <span>
                {children}
            </span>
            <div className="loader">
                <Loader />
            </div>
        </button>
    );
}