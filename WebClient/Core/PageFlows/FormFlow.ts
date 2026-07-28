import type { IFormHandler } from "../FormHandlers/IFormHandler";
import type { HintKeepForm } from "../Forms/ViewModels";
import { type SyntheticEvent, useCallback, useMemo, useRef, useState } from "react";
import { type ResolvableSimpleDependency, useDependency } from "react-model-view-viewmodel";
import { Notifications } from "../Notifications";
import { useShowConfirmationPrompt, type IConfirmationPromptOptions } from "../Prompt";

export type IFormFlow<TForm extends HintKeepForm, TResult> =
    IFormFlowReadyState<TForm, TResult>
    | IFormFlowSubmittingState<TForm, TResult>
    | IFormFlowSubmittedState<TForm, TResult>
    | IFormFlowFaultedState<TForm, TResult>;

export interface IFormFlowOptions<TForm extends HintKeepForm, TResult> {
    readonly form: ResolvableSimpleDependency<TForm>;
    readonly formHandler: ResolvableSimpleDependency<IFormHandler<TForm, TResult>>;

    readonly notifications?: {
        readonly successMessage?: string;
        readonly faultedMessage?: string;
    };

    readonly confirmationPrompt?: IConfirmationPromptOptions;
}

export function useFormFlow<TForm extends HintKeepForm, TResult>(options: IFormFlowOptions<TForm, TResult>): IFormFlow<TForm, TResult> {
    const {
        form: formDependency,
        formHandler: formHandlerDependency,

        notifications: {
            successMessage,
            faultedMessage
        } = {},

        confirmationPrompt
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

                        if (successMessage !== null && successMessage !== undefined)
                            notifications.add({ message: successMessage });
                    }
                    else {
                        setState("ready");
                    }
                }
                catch (error) {
                    errorRef.current = error instanceof Error ? error : new Error(typeof error === "string" ? error : JSON.stringify(error));
                    setState("faulted");

                    if (faultedMessage !== null && faultedMessage !== undefined)
                        notifications.add({
                            message: faultedMessage,
                            type: "error"
                        });
                    else
                        notifications.add({
                            message: error instanceof DOMException ? error.message : error as any,
                            type: "error"
                        });
                }
        },
        [form, formHandler, resultRef, notifications, successMessage, faultedMessage, setState]
    );

    const onConfirm = confirmationPrompt?.onConfirm;
    const onConfirmCallback = useCallback(
        () => {
            onConfirm && onConfirm();
            submitAsyncCallback();
        },
        [onConfirm, submitAsyncCallback]
    );

    const showConfirmationPrompt = useShowConfirmationPrompt({
        ...confirmationPrompt,
        onConfirm: onConfirmCallback
    });

    const formFlow = useMemo<IFormFlowBaseState<TForm, TResult>>(
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

            submitAsync: confirmationPrompt ? showConfirmationPrompt : submitAsyncCallback
        }),
        [state, form, resultRef, confirmationPrompt, showConfirmationPrompt, submitAsyncCallback]
    );

    return formFlow as IFormFlow<TForm, TResult>;
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