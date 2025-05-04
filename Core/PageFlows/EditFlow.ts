import type { IConfirmationPromptOptions } from "../../Pages/Prompt";
import type { IDataSource, IEntityScoped } from "../DataSources";
import type { IFormHandler } from "../FormHandlers/IFormHandler";
import type { HintKeepForm } from "../Forms/ViewModels";
import { type SyntheticEvent, useEffect, useMemo, useRef, useState } from "react";
import { type IDependencyResolver, useDependencyResolver, type ResolvableSimpleDependency, type ViewModelType } from "react-model-view-viewmodel";
import { useDataSourceFlow } from "./DataSourceFlow";
import { useFormFlow } from "./FormFlow";

export type IEditFlow<TEntity, TForm extends HintKeepForm, TResult> =
    IEditFlowLoadingState<TEntity, TForm, TResult>
    | IEditFlowReadyState<TEntity, TForm, TResult>
    | IEditFlowSubmittingState<TEntity, TForm, TResult>
    | IEditFlowSubmittedState<TEntity, TForm, TResult>
    | IEditFlowFaultedState<TEntity, TForm, TResult>;

export interface IEditFlowOptions<TEntity, TForm extends HintKeepForm, TResult> {
    readonly entityId: string;
    readonly dataSource: ResolvableSimpleDependency<IDataSource<IEntityScoped, TEntity>>;
    readonly form: ViewModelType<TForm, [entity: TEntity | undefined, dependencyResolver: IDependencyResolver]>;
    readonly formHandler: ResolvableSimpleDependency<IFormHandler<TForm, TResult>>;

    readonly skipConfirmationPrompt?: boolean;
    readonly confirmationPrompt?: IConfirmationPromptOptions;
}

export function useEditFlow<TEntity, TForm extends HintKeepForm, TResult>(options: IEditFlowOptions<TEntity, TForm, TResult>): IEditFlow<TEntity, TForm, TResult> {
    const formRef = useRef<TForm | null>(null);
    const errorRef = useRef<Error | null>(null);
    const dependencyResolver = useDependencyResolver();
    const [state, setState] = useState<IEditFlow<TEntity, TForm, TResult>["state"]>("loading");

    const {
        entityId,
        dataSource,
        form: FormType,
        formHandler,
        skipConfirmationPrompt,
        confirmationPrompt
    } = options;

    if (formRef.current === null)
        formRef.current = new FormType(undefined, dependencyResolver);

    const dataSourceOptions = useMemo<IEntityScoped>(() => ({ id: entityId }), [entityId]);

    const {
        state: dataSourceFlowState,
        result: entity,
        error: dataSourceFlowError,
        loadAsync
    } = useDataSourceFlow({
        options: dataSourceOptions,
        dataSource
    });
    const {
        state: formFlowState,
        form,
        result,
        error: formFlowError,
        submitAsync
    } = useFormFlow({
        form: formRef.current as ResolvableSimpleDependency<TForm>,
        formHandler,
        skipConfirmationPrompt,
        confirmationPrompt
    });

    useEffect(
        () => {
            switch (dataSourceFlowState) {
                case "loading":
                    formRef.current = new FormType(undefined, dependencyResolver);
                    setState("loading");
                    break;

                case "ready":
                    formRef.current = new FormType(entity, dependencyResolver);
                    switch (formFlowState) {
                        case "ready":
                            setState("ready");
                            break;

                        case "submitting":
                            setState("submitting");
                            break;

                        case "submitted":
                            setState("submitted");
                            break;

                        case "faulted":
                            errorRef.current = formFlowError;
                            setState("faulted");
                            break;
                    }
                    break;

                case "faulted":
                    errorRef.current = dataSourceFlowError;
                    setState("faulted");
                    break;
            }
        },
        [dataSourceFlowState, formFlowState, dataSourceFlowError, formFlowError, entity, FormType, dependencyResolver, formRef]
    );

    const editFlow = useMemo<IEditFlowBaseState<TEntity, TForm, TResult>>(
        () => ({
            state,
            entity: state === "loading" || state === "faulted" ? undefined : entity,
            form,

            isProcessing: state === "loading" || state === "submitting",

            isLoading: state === "loading",
            isReady: (
                state === "ready"
                || state === "submitted"
            ),

            isSubmitting: state === "submitting",
            isSubmitted: state === "submitted",
            isFaulted: state === "faulted",

            result,
            error: errorRef.current,

            loadAsync,
            submitAsync
        }),
        [state, entity, form, result, errorRef, loadAsync, submitAsync]
    );

    return editFlow as IEditFlow<TEntity, TForm, TResult>;
}

interface IEditFlowBaseState<TEntity, TForm extends HintKeepForm, TResult> {
    readonly state: "loading" | "ready" | "submitting" | "submitted" | "faulted";
    readonly entity: TEntity | undefined;
    readonly form: TForm;

    readonly isProcessing: boolean;

    readonly isLoading: boolean;
    readonly isReady: boolean;

    readonly isSubmitting: boolean;
    readonly isSubmitted: boolean;
    readonly isFaulted: boolean;

    readonly result: TResult | null;
    readonly error: Error | null;

    loadAsync(event?: SyntheticEvent): Promise<void>;
    submitAsync(event?: SyntheticEvent): Promise<void>;
}

interface IEditFlowLoadingState<TEntity, TForm extends HintKeepForm, TResult> extends IEditFlowBaseState<TEntity, TForm, TResult> {
    readonly state: "loading";
    readonly entity: undefined;

    readonly isProcessing: true;

    readonly isLoading: true;
    readonly isReady: false;

    readonly isSubmitting: false;
    readonly isSubmitted: false;
    readonly isFaulted: false;

    readonly result: null;
    readonly error: null;
}

interface IEditFlowReadyState<TEntity, TForm extends HintKeepForm, TResult> extends IEditFlowBaseState<TEntity, TForm, TResult> {
    readonly state: "ready";
    readonly entity: TEntity;

    readonly isProcessing: false;

    readonly isLoading: false;
    readonly isReady: true;

    readonly isSubmitting: false;
    readonly isSubmitted: false;
    readonly isFaulted: false;

    readonly result: null;
    readonly error: null;
}

interface IEditFlowSubmittingState<TEntity, TForm extends HintKeepForm, TResult> extends IEditFlowBaseState<TEntity, TForm, TResult> {
    readonly state: "submitting";
    readonly entity: TEntity;

    readonly isProcessing: true;

    readonly isLoading: false;
    readonly isReady: false;

    readonly isSubmitting: true;
    readonly isSubmitted: false;
    readonly isFaulted: false;

    readonly result: null;
    readonly error: null;
}

interface IEditFlowSubmittedState<TEntity, TForm extends HintKeepForm, TResult> extends IEditFlowBaseState<TEntity, TForm, TResult> {
    readonly state: "submitted";
    readonly entity: TEntity;

    readonly isProcessing: false;

    readonly isLoading: false;
    readonly isReady: false;

    readonly isSubmitting: false;
    readonly isSubmitted: true;
    readonly isFaulted: false;

    readonly result: TResult;
    readonly error: null;
}

interface IEditFlowFaultedState<TEntity, TForm extends HintKeepForm, TResult> extends IEditFlowBaseState<TEntity, TForm, TResult> {
    readonly state: "faulted";
    readonly entity: undefined;

    readonly isProcessing: false;

    readonly isLoading: false;
    readonly isReady: false;

    readonly isSubmitting: false;
    readonly isSubmitted: false;
    readonly isFaulted: true;

    readonly result: null;
    readonly error: Error;
}