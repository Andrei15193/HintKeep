import type { IDataSource, IEntityScoped } from "../DataSources";
import type { IFormHandler } from "../FormHandlers/IFormHandler";
import type { HintKeepForm } from "../Forms";
import { type SyntheticEvent, useCallback, useEffect, useMemo, useState } from "react";
import { useDependency, useViewModel, type ResolvableSimpleDependency, type ViewModelType } from "react-model-view-viewmodel";
import { useDisplayFlow } from "./DisplayFlow";

export interface IEditFlow<TEntity, TForm> {
    readonly state: "loading" | "ready" | "faulted" | "submitting" | "submitted";

    readonly entity: TEntity | null;
    readonly form: TForm;

    readonly isLoading: boolean;
    readonly isReady: boolean;
    readonly isFaulted: boolean;

    readonly isSubmitting: boolean;
    readonly isSubmitted: boolean;

    readonly isCompleted: boolean;

    loadAsync(): Promise<void>;
    submitAsync(event?: SyntheticEvent<unknown>): Promise<void>;
}

export interface IEditFlowOptions<TEntity, TForm extends HintKeepForm> {
    readonly entityId: string;
    readonly dataSource: ResolvableSimpleDependency<IDataSource<IEntityScoped, TEntity>>;
    readonly form: ViewModelType<TForm, [entity: TEntity | null]>;
    readonly formHandler: ResolvableSimpleDependency<IFormHandler<TForm>>;
}

export function useEditFlow<TEntity, TForm extends HintKeepForm>(options: IEditFlowOptions<TEntity, TForm>): IEditFlow<TEntity, TForm> {
    const {
        entityId,
        dataSource,
        form: formDependency,
        formHandler: formHandlerDependency
    } = options;

    const [state, setState] = useState<IEditFlow<TEntity, TForm>["state"]>("loading");

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
            entity: state === "loading" || state === "faulted" ? null : entity,
            form,

            isLoading: state === "loading",
            isFaulted: state === "faulted",
            isReady: (
                state === "ready"
                || state === "submitted"
            ),

            isSubmitting: state === "submitting",
            isSubmitted: state === "submitted",

            isCompleted: state === "submitted",

            loadAsync,
            submitAsync: submitAsyncCallback
        }),
        [state, entity, form, loadAsync, submitAsyncCallback]
    );
}