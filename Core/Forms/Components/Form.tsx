import React, { type FormEvent, useMemo, type PropsWithChildren } from "react";
import { FormContext, type IFormContext } from "./FormContext";

export interface IFormProps {
    readonly isLoading: boolean;

    readonly className?: string;
    readonly disabled?: boolean;

    onSubmit(event: FormEvent<HTMLFormElement>): void;
}

export function blankSubmit(event: FormEvent<HTMLFormElement>): void {
    event.preventDefault();
}

export function Form(props: PropsWithChildren<IFormProps>): React.JSX.Element {
    const {
        isLoading,
        className,
        disabled,
        children,
        onSubmit
    } = props;

    const formContext = useMemo<IFormContext>(
        () => ({
            disabled: !!disabled
        }),
        [disabled]
    );

    if (isLoading)
        return (
            <>
                Loading
            </>
        );
    else
        return (
            <form
                autoComplete="off"
                autoCorrect="off"
                className={className}
                onSubmit={onSubmit}
            >
                <FormContext value={formContext}>
                    {children}
                </FormContext>
            </form>
        );
}