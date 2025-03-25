import type { IFormHandler } from "../FormHandlers/IFormHandler";
import type { HintKeepForm } from "../Forms";
import { type SyntheticEvent, useCallback, useMemo, useRef, useState } from "react";
import { useDependency, useViewModel, type ResolvableSimpleDependency, type ViewModelType } from "react-model-view-viewmodel";

export interface ICreateFlow<TForm extends HintKeepForm, TResult> {
    readonly state: "ready" | "faulted" | "submitting" | "submitted";

    readonly form: TForm;

    readonly isReady: boolean;
    readonly isFaulted: boolean;

    readonly isSubmitting: boolean;
    readonly isSubmitted: boolean;

    readonly isCompleted: boolean;

    readonly result: TResult | null;

    submitAsync(event?: SyntheticEvent<unknown>): Promise<void>;
}

export interface ICreateFlowOptions<TForm extends HintKeepForm, TResult> {
    readonly form: ViewModelType<TForm>;
    readonly formHandler: ResolvableSimpleDependency<IFormHandler<TForm, TResult>>;
}

export function useCreateFlow<TForm extends HintKeepForm, TResult>(options: ICreateFlowOptions<TForm, TResult>): ICreateFlow<TForm, TResult> {
    const {
        form: formDependency,
        formHandler: formHandlerDependency
    } = options;

    const resultRef = useRef<TResult | null>(null);
    const [state, setState] = useState<ICreateFlow<TForm, TResult>["state"]>("ready");

    const form = useViewModel(formDependency);
    const formHandler = useDependency(formHandlerDependency);

    const submitAsyncCallback = useCallback(
        async (event?: SyntheticEvent<unknown>) => {
            event?.preventDefault();

            form.validate();
            if (form.isValid)
                try {
                    setState("submitting");

                    resultRef.current = await formHandler.handleAsync(form);

                    if (form.isValid)
                        setState("submitted");
                    else
                        setState("ready");
                }
                catch (error) {
                    setState("faulted");
                    console.error(error);
                }
        },
        [form, formHandler, resultRef, setState]
    );

    return useMemo(
        () => ({
            state,
            form,

            isFaulted: state === "faulted",
            isReady: (
                state === "ready"
                || state === "submitted"
            ),

            isSubmitting: state === "submitting",
            isSubmitted: state === "submitted",

            isCompleted: state === "submitted",

            result: state === "submitted" ? resultRef.current : null,

            submitAsync: submitAsyncCallback
        }),
        [state, form, resultRef, submitAsyncCallback]
    );
}