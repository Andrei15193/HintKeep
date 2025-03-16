import type { IDataSource, IEntityScoped } from "../DataSources";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useDependency, type ResolvableSimpleDependency } from "react-model-view-viewmodel";

export interface IDisplayFlow<TEntity> {
    readonly state: "loading" | "ready" | "faulted";
    readonly entity: TEntity | null;

    readonly isLoading: boolean;
    readonly isReady: boolean;
    readonly isFaulted: boolean;
    readonly isCompleted: boolean;

    loadAsync(): Promise<void>;
}

export interface IDisplayFlowOptions<TEntity> {
    readonly entityId: string;
    readonly dataSource: ResolvableSimpleDependency<IDataSource<IEntityScoped, TEntity>>;
}

export function useDisplayFlow<TEntity>(options: IDisplayFlowOptions<TEntity>): IDisplayFlow<TEntity> {
    const {
        entityId,
        dataSource: dataSourceDependency
    } = options;

    const dataSource = useDependency(dataSourceDependency);

    const [state, setState] = useState<IDisplayFlow<TEntity>["state"]>("loading");
    const entityRef = useRef<TEntity | null>(null);

    const latestFetchTokenRef = useRef<unknown>(null);
    const loadAsyncCallback = useCallback(
        async () => {
            const fetchToken = latestFetchTokenRef.current = {};

            try {
                setState("loading");
                const entity = await dataSource.getDataAsync({ id: entityId });

                if (latestFetchTokenRef.current === fetchToken) {
                    entityRef.current = entity;
                    setState("ready");
                }
            }
            catch {
                if (latestFetchTokenRef.current === fetchToken)
                    setState("faulted");
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

    const entity = entityRef.current;

    return useMemo(
        () => ({
            state,
            entity,

            isLoading: state === "loading",
            isReady: state === "ready",
            isFaulted: state === "faulted",
            isCompleted: state === "ready" || state === "faulted",

            loadAsync: loadAsyncCallback
        }),
        [state, entity, loadAsyncCallback]
    );
}