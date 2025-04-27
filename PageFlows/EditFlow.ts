import type { IDataSource, IEntityScoped } from "../DataSources";
import type { IFormHandler } from "../FormHandlers/IFormHandler";
import type { HintKeepForm } from "../Forms";
import { type SyntheticEvent, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useDependency, useViewModel, type ResolvableSimpleDependency, type ViewModelType } from "react-model-view-viewmodel";
import { useBlocker } from "react-router";
import { Notifications } from "../Pages/Notifications";
import { type IConfirmationPromptOptions, useShowConfirmationPrompt } from "../Pages/Prompt";
import { useDisplayFlow } from "./DisplayFlow";

export interface IEditFlow<TEntity, TForm extends HintKeepForm, TResult> {
    readonly state: "loading" | "ready" | "faulted" | "submitting" | "submitted";

    readonly entity: TEntity | null;
    readonly form: TForm;

    readonly isLoading: boolean;
    readonly isProcessing: boolean;
    readonly isReady: boolean;
    readonly isFaulted: boolean;

    readonly isSubmitting: boolean;
    readonly isSubmitted: boolean;

    readonly isCompleted: boolean;

    readonly result: TResult | null;

    loadAsync(): Promise<void>;
    submitAsync(event?: SyntheticEvent): Promise<void>;
}

export interface IEditFlowOptions<TEntity, TForm extends HintKeepForm, TResult> {
    readonly entityId: string;
    readonly dataSource: ResolvableSimpleDependency<IDataSource<IEntityScoped, TEntity>>;
    readonly form: ViewModelType<TForm, [entity: TEntity | null]>;
    readonly formHandler: ResolvableSimpleDependency<IFormHandler<TForm, TResult>>;

    readonly skipConfirmationPrompt?: boolean;
    readonly confirmationPrompt?: IConfirmationPromptOptions;
}

export function useEditFlow<TEntity, TForm extends HintKeepForm, TResult>(options: IEditFlowOptions<TEntity, TForm, TResult>): IEditFlow<TEntity, TForm, TResult> {
    const {
        entityId,
        dataSource,
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
    const [state, setState] = useState<IEditFlow<TEntity, TForm, TResult>["state"]>("loading");

    const {
        state: loadingState,
        entity,
        loadAsync
    } = useDisplayFlow({
        entityId,
        dataSource
    });

    const form = useViewModel(formDependency, [entity]);
    const formHandler = useDependency(formHandlerDependency);

    useEffect(
        () => {
            switch (loadingState) {
                case "loading":
                    setState("loading");
                    break;

                case "faulted":
                    setState("faulted");
                    break;

                case "ready":
                    setState("ready");
                    break;
            }
        },
        [loadingState]
    );

    const submitAsyncCallback = useCallback(
        async (event?: SyntheticEvent) => {
            event?.preventDefault();

            form.validate();
            if (form.isValid)
                try {
                    setState("submitting");

                    resultRef.current = await formHandler.handleAsync(form);

                    setState("submitted");
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

    return useMemo<IEditFlow<TEntity, TForm, TResult>>(
        () => ({
            state,
            entity: state === "loading" || state === "faulted" ? null : entity,
            form,

            isLoading: state === "loading",
            isProcessing: state === "loading" || state === "submitting",
            isFaulted: state === "faulted",
            isReady: (
                state === "ready"
                || state === "submitted"
            ),

            isSubmitting: state === "submitting",
            isSubmitted: state === "submitted",

            isCompleted: state === "submitted",

            result: resultRef.current,

            loadAsync,
            submitAsync: submitAsyncCallback
        }),
        [state, entity, form, resultRef, loadAsync, submitAsyncCallback]
    );
}