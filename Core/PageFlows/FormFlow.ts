import type { IFormHandler } from "../FormHandlers/IFormHandler";
import type { HintKeepForm } from "../Forms/ViewModels";
import { type SyntheticEvent, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { type ResolvableSimpleDependency, useDependency } from "react-model-view-viewmodel";
import { useBlocker } from "react-router";
import { useShowConfirmationPrompt, type IConfirmationPromptOptions } from "../../Pages/Prompt";
import { Notifications } from "../Notifications";

export type IFormFlow<TForm extends HintKeepForm, TResult> =
    IFormFlowReadyState<TForm, TResult>
    | IFormFlowSubmittingState<TForm, TResult>
    | IFormFlowSubmittedState<TForm, TResult>
    | IFormFlowFaultedState<TForm, TResult>;

export interface IFormFlowOptions<TForm extends HintKeepForm, TResult> {
    readonly form: ResolvableSimpleDependency<TForm>;
    readonly formHandler: ResolvableSimpleDependency<IFormHandler<TForm, TResult>>;

    readonly skipConfirmationPrompt?: boolean;
    readonly confirmationPrompt?: IConfirmationPromptOptions;
}

export function useFormFlow<TForm extends HintKeepForm, TResult>(options: IFormFlowOptions<TForm, TResult>): IFormFlow<TForm, TResult> {
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
    const errorRef = useRef<Error | null>(null);
    const [state, setState] = useState<IFormFlow<TForm, TResult>["state"]>("ready");

    const form = useDependency(formDependency);
    const formHandler = useDependency(formHandlerDependency);

    const submitAsyncCallback = useCallback(
        async (event?: SyntheticEvent) => {
            event?.preventDefault();

            form.validate();
            if (form.isValid)
                try {
                    resultRef.current = null;
                    errorRef.current = null;
                    setState("submitting");

                    const result = await formHandler.handleAsync(form);
                    if (form.isValid && result !== null && result !== undefined) {
                        resultRef.current = result;
                        setState("submitted");
                    }
                    else {
                        setState("ready");
                    }
                }
                catch (error) {
                    errorRef.current = error instanceof Error ? error : new Error(typeof error === "string" ? error : JSON.stringify(error));
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
                    event.returnValue = ""; // Chrome requires returnValue to be set.
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

    const editForm = useMemo<IFormFlowBaseState<TForm, TResult>>(
        () => ({
            state,

            isReady: (
                state === "ready"
                || state === "submitted"
            ),
            isSubmitting: state === "submitting",
            isSubmitted: state === "submitted",
            isFaulted: state === "faulted",

            form,
            result: state === "submitted" ? resultRef.current : null,
            error: state === "faulted" ? errorRef.current : null,

            submitAsync: submitAsyncCallback
        }),
        [state, form, resultRef, submitAsyncCallback]
    );

    return editForm as IFormFlow<TForm, TResult>;
}

interface IFormFlowBaseState<TForm extends HintKeepForm, TResult> {
    readonly state: "ready" | "submitting" | "submitted" | "faulted";
    readonly form: TForm;

    readonly isReady: boolean;
    readonly isSubmitting: boolean;
    readonly isSubmitted: boolean;
    readonly isFaulted: boolean;

    readonly result: TResult | null;
    readonly error: Error | null;

    submitAsync(event?: SyntheticEvent): Promise<void>;
}

interface IFormFlowReadyState<TForm extends HintKeepForm, TResult> extends IFormFlowBaseState<TForm, TResult> {
    readonly state: "ready";

    readonly isReady: true;
    readonly isSubmitting: false;
    readonly isSubmitted: false;
    readonly isFaulted: false;

    readonly result: null;
    readonly error: null;
}

interface IFormFlowSubmittingState<TForm extends HintKeepForm, TResult> extends IFormFlowBaseState<TForm, TResult> {
    readonly state: "submitting";

    readonly isReady: false;
    readonly isSubmitting: true;
    readonly isSubmitted: false;
    readonly isFaulted: false;

    readonly result: null;
    readonly error: null;
}

interface IFormFlowSubmittedState<TForm extends HintKeepForm, TResult> extends IFormFlowBaseState<TForm, TResult> {
    readonly state: "submitted";

    readonly isReady: true;
    readonly isSubmitting: false;
    readonly isSubmitted: true;
    readonly isFaulted: false;

    readonly result: TResult;
    readonly error: null;
}

interface IFormFlowFaultedState<TForm extends HintKeepForm, TResult> extends IFormFlowBaseState<TForm, TResult> {
    readonly state: "faulted";

    readonly isReady: false;
    readonly isSubmitting: false;
    readonly isSubmitted: false;
    readonly isFaulted: true;

    readonly result: TResult;
    readonly error: Error;
}