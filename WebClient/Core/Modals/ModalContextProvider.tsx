import React, { type Key, type ReactNode, type PropsWithChildren, useRef, useState, useCallback, useMemo } from "react";
import { type IModalContext, ModalContext } from "./ModalContext";

export interface IModalContextProviderProps extends PropsWithChildren {
}

export function ModalContextProvider({ children }: IModalContextProviderProps): React.JSX.Element {
    const lastModalNumberRef = useRef(Number.MIN_SAFE_INTEGER);
    const [modals, setModals] = useState<readonly (readonly [key: Key, content: ReactNode])[]>([]);

    const showCallback = useCallback(
        (content: ReactNode): Key => {
            const key = lastModalNumberRef.current = (
                lastModalNumberRef.current < Number.MAX_SAFE_INTEGER
                    ? lastModalNumberRef.current + 1
                    : Number.MIN_SAFE_INTEGER
            );

            setModals((modals) => [
                ...modals,
                [key, content]
            ]);

            return key;
        },
        [lastModalNumberRef, setModals]
    );

    const updateCallback = useCallback(
        (key: Key, content: ReactNode): void => {
            setModals((modals) => modals.map((pair) => (
                pair[0] === key
                    ? [key, content]
                    : pair
            )));
        },
        [setModals]
    );

    const hideCallback = useCallback(
        (key: Key): void => {
            setModals((modals) => modals.filter((pair) => pair[0] !== key));
        },
        [setModals]
    );

    const modalContext = useMemo(
        (): IModalContext => {
            return {
                modals,

                show: showCallback,
                update: updateCallback,
                hide: hideCallback
            };
        },
        [modals, showCallback, updateCallback, hideCallback]
    );

    return (
        <ModalContext.Provider value={modalContext}>
            {children}
        </ModalContext.Provider>
    );
}