import type { IDataSource, IEntityScoped } from "../DataSources";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useDependency, type ResolvableSimpleDependency } from "react-model-view-viewmodel";

export type IDataSourceFlow<TEntity> =
    IDataSourceFlowLoadingState<TEntity>
    | IDataSourceFlowReadyState<TEntity>
    | IDataSourceFlowFaultedState<TEntity>;

export interface IDataSourceFlowOptions<TEntity> {
    readonly entityId: string;
    readonly dataSource: ResolvableSimpleDependency<IDataSource<IEntityScoped, TEntity>>;
}

export function useDataSourceFlow<TEntity>(options: IDataSourceFlowOptions<TEntity>): IDataSourceFlow<TEntity> {
    const {
        entityId,
        dataSource: dataSourceDependency
    } = options;

    const dataSource = useDependency(dataSourceDependency);

    const entityRef = useRef<TEntity | null>(null);
    const errorRef = useRef<Error | null>(null);
    const [state, setState] = useState<IDataSourceFlow<TEntity>["state"]>("loading");

    const latestFetchTokenRef = useRef<unknown>(null);
    const loadAsyncCallback = useCallback(
        async () => {
            const fetchToken = latestFetchTokenRef.current = {};

            try {
                entityRef.current = null;
                errorRef.current = null;
                setState("loading");
                const entity = await dataSource.getDataAsync({ id: entityId });

                if (latestFetchTokenRef.current === fetchToken) {
                    entityRef.current = entity;
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
        [entityId, dataSource, entityRef, latestFetchTokenRef, setState]
    );

    useEffect(
        () => {
            loadAsyncCallback();
        },
        [loadAsyncCallback]
    );

    const dataSourceFlow = useMemo<IDataSourceFlowBaseState<TEntity>>(
        () => ({
            state,

            isLoading: state === "loading",
            isReady: state === "ready",
            isFaulted: state === "faulted",

            entity: state === "ready" ? entityRef.current : null,
            error: state === "faulted" ? errorRef.current : null,

            loadAsync: loadAsyncCallback
        }),
        [state, loadAsyncCallback]
    );

    return dataSourceFlow as IDataSourceFlow<TEntity>;
}

interface IDataSourceFlowBaseState<TEntity> {
    readonly state: "loading" | "ready" | "faulted";

    readonly isLoading: boolean;
    readonly isReady: boolean;
    readonly isFaulted: boolean;

    readonly entity: TEntity | null;
    readonly error: Error | null;

    loadAsync(): Promise<void>;
}

interface IDataSourceFlowLoadingState<TEntity> extends IDataSourceFlowBaseState<TEntity> {
    readonly state: "loading";

    readonly isLoading: true;
    readonly isReady: false;
    readonly isFaulted: false;

    readonly entity: null;
    readonly error: null;
}

interface IDataSourceFlowReadyState<TEntity> extends IDataSourceFlowBaseState<TEntity> {
    readonly state: "ready";

    readonly isLoading: false;
    readonly isReady: true;
    readonly isFaulted: false;

    readonly entity: TEntity;
    readonly error: null;
}

interface IDataSourceFlowFaultedState<TEntity> extends IDataSourceFlowBaseState<TEntity> {
    readonly state: "faulted";

    readonly isLoading: false;
    readonly isReady: false;
    readonly isFaulted: true;

    readonly entity: null;
    readonly error: Error;
}