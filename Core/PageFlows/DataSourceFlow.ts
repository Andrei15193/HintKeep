import type { IDataSource } from "../DataSources";
import { type SyntheticEvent, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useDependency, type ResolvableSimpleDependency } from "react-model-view-viewmodel";

export type IDataSourceFlow<TOptions, TResult> =
    IDataSourceFlowLoadingState<TOptions, TResult>
    | IDataSourceFlowReadyState<TOptions, TResult>
    | IDataSourceFlowFaultedState<TOptions, TResult>;

export interface IDataSourceFlowOptions<TOptions extends object, TResult> {
    readonly options: TOptions | (() => TOptions);
    readonly dataSource: ResolvableSimpleDependency<IDataSource<TOptions, TResult>>;
}

export function useDataSourceFlow<TOptions extends object, TResult>(options: IDataSourceFlowOptions<TOptions, TResult>): IDataSourceFlow<TOptions, TResult> {
    const {
        options: dataSourceOptions,
        dataSource: dataSourceDependency
    } = options;

    const dataSource = useDependency(dataSourceDependency);

    const resultRef = useRef<TResult | undefined>(undefined);
    const errorRef = useRef<Error | null>(null);
    const [state, setState] = useState<IDataSourceFlow<TOptions, TResult>["state"]>("loading");

    const latestFetchTokenRef = useRef<unknown>(null);
    const loadAsyncCallback = useCallback(
        async (event?: SyntheticEvent, optionOverwrites?: Partial<TOptions>) => {
            event?.preventDefault();

            const fetchToken = latestFetchTokenRef.current = {};
            try {
                resultRef.current = undefined;
                errorRef.current = null;
                setState("loading");
                const result = await dataSource.getDataAsync(Object.assign(
                    {},
                    typeof dataSourceOptions === "function" ? dataSourceOptions() : dataSourceOptions,
                    optionOverwrites
                ));

                if (latestFetchTokenRef.current === fetchToken) {
                    resultRef.current = result;
                    setState("ready");
                }
            }
            catch (error) {
                if (latestFetchTokenRef.current === fetchToken) {
                    errorRef.current = error instanceof Error ? error : new Error(typeof error === "string" ? error : JSON.stringify(error));
                    setState("faulted");
                }
            }
        },
        [dataSourceOptions, dataSource, resultRef, latestFetchTokenRef, setState]
    );

    useEffect(
        () => {
            loadAsyncCallback();
        },
        [loadAsyncCallback]
    );

    const dataSourceFlow = useMemo<IDataSourceFlowBaseState<TOptions, TResult>>(
        () => ({
            state,

            isLoading: state === "loading",
            isReady: state === "ready",
            isFaulted: state === "faulted",

            result: state === "ready" ? resultRef.current : undefined,
            error: state === "faulted" ? errorRef.current : null,

            loadAsync: loadAsyncCallback
        }),
        [state, loadAsyncCallback]
    );

    return dataSourceFlow as IDataSourceFlow<TOptions, TResult>;
}

interface IDataSourceFlowBaseState<TOptions, TResult> {
    readonly state: "loading" | "ready" | "faulted";

    readonly isLoading: boolean;
    readonly isReady: boolean;
    readonly isFaulted: boolean;

    readonly result: TResult | undefined;
    readonly error: Error | null;

    loadAsync(event?: SyntheticEvent, options?: Partial<TOptions>): Promise<void>;
}

interface IDataSourceFlowLoadingState<TOptions, TResult> extends IDataSourceFlowBaseState<TOptions, TResult> {
    readonly state: "loading";

    readonly isLoading: true;
    readonly isReady: false;
    readonly isFaulted: false;

    readonly result: undefined;
    readonly error: null;
}

interface IDataSourceFlowReadyState<TOptions, TResult> extends IDataSourceFlowBaseState<TOptions, TResult> {
    readonly state: "ready";

    readonly isLoading: false;
    readonly isReady: true;
    readonly isFaulted: false;

    readonly result: TResult;
    readonly error: null;
}

interface IDataSourceFlowFaultedState<TOptions, TResult> extends IDataSourceFlowBaseState<TOptions, TResult> {
    readonly state: "faulted";

    readonly isLoading: false;
    readonly isReady: false;
    readonly isFaulted: true;

    readonly result: undefined;
    readonly error: Error;
}