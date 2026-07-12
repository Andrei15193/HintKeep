import { type Key, type ReactNode, createContext } from "react";

export const ModalContext = createContext<IModalContext>(null!);

export interface IModalContext {
    readonly modals: readonly (readonly [key: Key, content: ReactNode])[];

    show(content: ReactNode): Key;
    update(key: Key, content: ReactNode): void;
    hide(key: Key): void;
}