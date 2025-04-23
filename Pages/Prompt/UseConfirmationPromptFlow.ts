import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { ConfirmationPromptContext, type IConfirmationPrompt } from "./ConfirmationPromptContext";

export interface IConfirmationPromptFlow {
    readonly state: "hidden" | "visible" | "confirmed" | "dismissed";

    readonly isHidden: boolean;
    readonly isVisible: boolean;
    readonly isConfirmed: boolean;
    readonly isDismissed: boolean;

    show(): void;
    confirm(): void;
    dismiss(): void;
}

export interface IConfirmationPromptFlowOptions {
    readonly message: string;
    readonly confirmButtonLabel?: string;
    readonly dismissButtonLabel?: string;
}

export function useConfirmationPromptFlow({ message, confirmButtonLabel, dismissButtonLabel }: IConfirmationPromptFlowOptions): IConfirmationPromptFlow {
    const [state, setState] = useState<IConfirmationPromptFlow["state"]>("hidden");
    const { show, dismiss } = useContext(ConfirmationPromptContext)!;

    const showCallback = useCallback(() => setState("visible"), [setState]);
    const confirmCallback = useCallback(() => setState("confirmed"), [setState]);
    const dismissCallback = useCallback(() => setState("dismissed"), [setState]);

    const confirmationPromptFlow = useMemo<IConfirmationPromptFlow>(
        () => ({
            state,
            message,

            isHidden: state === "hidden",
            isVisible: state === "visible",
            isConfirmed: state === "confirmed",
            isDismissed: state === "dismissed",

            show: showCallback,
            confirm: confirmCallback,
            dismiss: dismissCallback
        }),
        [state, message, showCallback, confirmCallback, dismissCallback]
    );

    const confirmationPrompt = useMemo<IConfirmationPrompt>(
        () => ({
            message,
            confirmButtonLabel,
            dismissButtonLabel,
            confirm: confirmCallback,
            dismiss: dismissCallback
        }),
        [message, confirmButtonLabel, dismissButtonLabel, confirmCallback, dismissCallback]
    );

    useEffect(
        () => {
            switch (state) {
                case "hidden":
                    dismiss();
                    break;

                case "visible":
                    show(confirmationPrompt);
                    break;

                case "confirmed":
                    dismiss();
                    break;

                case "dismissed":
                    dismiss();
                    break;
            }
        },
        [state, confirmationPrompt, show, dismiss]
    );

    return confirmationPromptFlow;
}