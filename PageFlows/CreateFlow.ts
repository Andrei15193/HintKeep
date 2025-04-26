import type { IFormHandler } from "../FormHandlers/IFormHandler";
import type { HintKeepForm } from "../Forms";
import { type SyntheticEvent, useCallback, useMemo, useRef, useState } from "react";
import { useDependency, useViewModel, type ResolvableSimpleDependency, type ViewModelType } from "react-model-view-viewmodel";
import { Notifications } from "../Pages/Notifications";
import { useConfirmationPrompt, type IConfirmationPromptOptions } from "../Pages/Prompt";

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
    dismissAsync(event?: SyntheticEvent<unknown>): Promise<void>;
}

export interface ICreateFlowOptions<TForm extends HintKeepForm, TResult> {
    readonly form: ViewModelType<TForm>;
    readonly formHandler: ResolvableSimpleDependency<IFormHandler<TForm, TResult>>;

    readonly confirmationPrompt?: IConfirmationPromptOptions;
}

export function useCreateFlow<TForm extends HintKeepForm, TResult>(options: ICreateFlowOptions<TForm, TResult>): ICreateFlow<TForm, TResult> {
    const {
        form: formDependency,
        formHandler: formHandlerDependency,
        confirmationPrompt = {}
    } = options;
    const notifications = useDependency(Notifications);
    const { show: showConfirmationPrompt } = useConfirmationPrompt();

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
                    notifications.add({
                        message: error instanceof DOMException ? error.message : error,
                        type: "error"
                    });
                }
        },
        [form, formHandler, resultRef, notifications, setState]
    );

    const {
        message,
        confirmButtonLabel,
        dismissButtonLabel,
        onConfirm,
        onDismiss
    } = confirmationPrompt;
    const dismissCallback = useCallback(
        () => new Promise<void>((resolve) => showConfirmationPrompt({
            message,
            confirmButtonLabel,
            dismissButtonLabel,
            onConfirm() {
                onConfirm && onConfirm();
                resolve();
            },
            onDismiss() {
                onDismiss && onDismiss();
                resolve();
            }
        })),
        [message, confirmButtonLabel, dismissButtonLabel, onConfirm, onDismiss, showConfirmationPrompt]
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

            submitAsync: submitAsyncCallback,
            dismissAsync: dismissCallback
        }),
        [state, form, resultRef, submitAsyncCallback, dismissCallback]
    );
}