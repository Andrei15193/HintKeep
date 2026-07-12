import { type PropsWithChildren, useEffect } from "react";
import { useModal } from "./UseModal";

export interface IModalProps extends PropsWithChildren {
    readonly isVisible: boolean;
}

export function Modal({ isVisible, children }: IModalProps): null {
    const { show, hide } = useModal(children);

    useEffect(
        () => {
            if (isVisible)
                show();
            else
                hide();
        },
        [isVisible, show, hide]
    );

    return null;
}