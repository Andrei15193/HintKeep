import type { IDataSourceFlow, IEditFlow, IFormFlow } from "../../PageFlows";
import type { HintKeepForm } from "../ViewModels";
import React, { useMemo, type PropsWithChildren } from "react";
import { FormContext, type IFormContext } from "./FormContext";

export interface IFormProps {
    readonly pageFlow: IFormFlow<HintKeepForm, unknown> | IEditFlow<unknown, HintKeepForm, unknown> | IDataSourceFlow<unknown, unknown>;

    readonly withoutLoadingState?: boolean;
    readonly className?: string;
    readonly disabled?: boolean;
}

export function Form({ pageFlow, withoutLoadingState, className, disabled, children }: PropsWithChildren<IFormProps>): React.JSX.Element {
    const {
        isLoading,
        isSubmitting,
        isProcessing = isLoading || isSubmitting,
        loadAsync,
        submitAsync = loadAsync
    } = pageFlow as IEditFlow<unknown, HintKeepForm, unknown>;

    const formContext = useMemo<IFormContext>(
        () => ({
            disabled: !!disabled
        }),
        [disabled]
    );

    if (!withoutLoadingState && isProcessing)
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
                onSubmit={submitAsync}
            >
                <FormContext value={formContext}>
                    {children}
                </FormContext>
            </form>
        );
}