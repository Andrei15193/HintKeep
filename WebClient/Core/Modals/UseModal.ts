import { type ReactNode, type Key, useContext, useState, useCallback, useEffect, useLayoutEffect, useMemo } from "react";
import { ModalContext } from "./ModalContext";

export interface IModal {
    readonly isVisible: boolean;
    readonly isHidden: boolean;

    show(): void;
    hide(): void;
    toggle(): void;
}

export function useModal(content: ReactNode): IModal {
    const { show, update, hide } = useContext(ModalContext);
    const [modalKey, setModalKey] = useState<Key | null>(null);

    const showCallback = useCallback(
        () => {
            if (modalKey === null)
                setModalKey(show(content));
        },
        [modalKey, content, show, setModalKey]
    );

    const hideCallback = useCallback(
        () => {
            if (modalKey !== null) {
                setModalKey(null);
                hide(modalKey);
            }
        },
        [modalKey, hide, setModalKey]
    );

    const toggleCallback = useCallback(
        () => {
            if (modalKey === null)
                showCallback();
            else
                hideCallback();
        },
        [modalKey, showCallback, hideCallback]
    );

    useEffect(
        () => {
            if (modalKey !== null)
                update(modalKey, content);
        },
        [modalKey, content, update]
    );

    useLayoutEffect(() => hideCallback, [hideCallback]);

    return useMemo(
        (): IModal => ({
            isVisible: modalKey !== null,
            isHidden: modalKey === null,

            show: showCallback,
            hide: hideCallback,
            toggle: toggleCallback
        }),
        [modalKey, showCallback, hideCallback, toggleCallback]
    );
}