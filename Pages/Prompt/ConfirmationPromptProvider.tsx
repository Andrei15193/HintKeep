import React, { type PropsWithChildren, useCallback, useMemo, useState } from "react";
import { type IConfirmationPrompt, type IConfirmationPromptContext, type IConfirmationPromptOptions, ConfirmationPromptContext } from "./ConfirmationPromptContext";

export function ConfirmationPromptProvider({ children }: PropsWithChildren<{}>): React.JSX.Element {
    const [confirmationPrompt, setConfirmationPrompt] = useState<IConfirmationPrompt | null>(null);

    const showCallback = useCallback(
        (options?: IConfirmationPromptOptions) => {
            setConfirmationPrompt({
                options,

                confirm() {
                    const onConfirm = options?.onConfirm;
                    onConfirm && onConfirm();

                    setConfirmationPrompt(null);
                },

                dismiss() {
                    const onDismiss = options?.onDismiss;
                    onDismiss && onDismiss();

                    setConfirmationPrompt(null);
                }
            });
        },
        [setConfirmationPrompt]
    );

    const confirmationPromptContext = useMemo<IConfirmationPromptContext>(
        () => ({
            confirmationPrompt,

            show: showCallback
        }),
        [confirmationPrompt, showCallback]
    );

    return (
        <ConfirmationPromptContext.Provider
            value={confirmationPromptContext}
            children={children}
        />
    );
}