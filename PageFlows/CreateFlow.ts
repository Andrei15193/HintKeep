import type { IFormHandler } from "../FormHandlers/IFormHandler";
import type { HintKeepForm } from "../Forms";
import { type SyntheticEvent, useCallback, useMemo, useState } from "react";
import { useDependency, useViewModel, type ResolvableSimpleDependency, type ViewModelType } from "react-model-view-viewmodel";

export interface ICreateFlow<TForm> {
    readonly state: "ready" | "faulted" | "submitting" | "submitted";

    readonly form: TForm;

    readonly isReady: boolean;
    readonly isFaulted: boolean;

    readonly isSubmitting: boolean;
    readonly isSubmitted: boolean;

    readonly isCompleted: boolean;

    submitAsync(event?: SyntheticEvent<unknown>): Promise<void>;
}

export interface ICreateFlowOptions<TForm extends HintKeepForm> {
    readonly form: ViewModelType<TForm>;
    readonly formHandler: ResolvableSimpleDependency<IFormHandler<TForm>>;
}

export function useCreateFlow<TForm extends HintKeepForm>(options: ICreateFlowOptions<TForm>): ICreateFlow<TForm> {
    const {
        form: formDependency,
        formHandler: formHandlerDependency
    } = options;

    const [state, setState] = useState<ICreateFlow<TForm>["state"]>("ready");

    const form = useViewModel(formDependency);
    const formHandler = useDependency(formHandlerDependency);

    const submitAsyncCallback = useCallback(
        async (event?: SyntheticEvent<unknown>) => {
            event?.preventDefault();

            form.validate();
            if (form.isValid)
                try {
                    setState("submitting");

                    await formHandler.handleAsync(form);

                    setState("submitted");
                }
                catch {
                    setState("faulted");
                }
        },
        [form, formHandler, setState]
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

            submitAsync: submitAsyncCallback
        }),
        [state, form, submitAsyncCallback]
    );
}