import type { IFormHandler } from "../FormHandlers/IFormHandler";
import type { HintKeepForm } from "../Forms";
import { type SyntheticEvent, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useDependency, useViewModel, type ResolvableSimpleDependency, type ViewModelType } from "react-model-view-viewmodel";
import { useBlocker } from "react-router";
import { Notifications } from "../Pages/Notifications";
import { type IConfirmationPromptOptions, useShowConfirmationPrompt } from "../Pages/Prompt";

export interface ICreateFlow<TForm extends HintKeepForm, TResult> {
    readonly state: "ready" | "faulted" | "submitting" | "submitted";

    readonly form: TForm;

    readonly isProcessing: boolean;
    readonly isReady: boolean;
    readonly isFaulted: boolean;

    readonly isSubmitting: boolean;
    readonly isSubmitted: boolean;

    readonly isCompleted: boolean;

    readonly result: TResult | null;

    submitAsync(event?: SyntheticEvent): Promise<void>;
}

export interface ICreateFlowOptions<TForm extends HintKeepForm, TResult> {
    readonly form: ViewModelType<TForm>;
    readonly formHandler: ResolvableSimpleDependency<IFormHandler<TForm, TResult>>;

    readonly skipConfirmationPrompt?: boolean;
    readonly confirmationPrompt?: IConfirmationPromptOptions;
}

export function useCreateFlow<TForm extends HintKeepForm, TResult>(options: ICreateFlowOptions<TForm, TResult>): ICreateFlow<TForm, TResult> {
    const {
        form: formDependency,
        formHandler: formHandlerDependency,
        skipConfirmationPrompt,
        confirmationPrompt: {
            onConfirm,
            onDismiss,
            ...otherConfirmationPromptOptions
        } = {}
    } = options;
    const notifications = useDependency(Notifications);

    const resultRef = useRef<TResult | null>(null);
    const [state, setState] = useState<ICreateFlow<TForm, TResult>["state"]>("ready");

    const form = useViewModel(formDependency);
    const formHandler = useDependency(formHandlerDependency);

    const submitAsyncCallback = useCallback(
        async (event?: SyntheticEvent) => {
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

    const shouldBlockNavigation = !skipConfirmationPrompt && state !== "submitted";
    const blocker = useBlocker(shouldBlockNavigation);
    const onConfirmCallback = useCallback(
        () => {
            onConfirm && onConfirm();
            blocker.state === "blocked" && blocker.proceed();
        },
        [blocker, onConfirm]
    );
    const onDismissCallback = useCallback(
        () => {
            onDismiss && onDismiss();
            blocker.state === "blocked" && blocker.reset();
        },
        [blocker, onDismiss]
    );

    const showDiscardConfirmationPrompt = useShowConfirmationPrompt({
        onConfirm: onConfirmCallback,
        onDismiss: onDismissCallback,
        ...otherConfirmationPromptOptions
    });

    useEffect(
        () => {
            if (shouldBlockNavigation) {
                const beforeUnloadEventHandler = (event: BeforeUnloadEvent) => {
                    event.preventDefault();
                    // Chrome requires returnValue to be set.
                    event.returnValue = "";
                };
                window.addEventListener("beforeunload", beforeUnloadEventHandler);

                return () => {
                    window.removeEventListener("beforeunload", beforeUnloadEventHandler);
                };
            }
            else
                return undefined;
        },
        [shouldBlockNavigation]
    );

    useEffect(
        () => {
            if (blocker.state === "blocked")
                showDiscardConfirmationPrompt();
        },
        [blocker, showDiscardConfirmationPrompt]
    );

    return useMemo<ICreateFlow<TForm, TResult>>(
        () => ({
            state,
            form,

            isProcessing: state === "submitting",
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