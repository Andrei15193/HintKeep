import React, { type PropsWithChildren, useCallback, useMemo, useState } from "react";
import { type IConfirmationPrompt, type IConfirmationPromptContext, type IConfirmationPromptOptions, ConfirmationPromptContext } from "./ConfirmationPromptContext";

export function ConfirmationPromptProvider({ children }: PropsWithChildren<{}>): React.JSX.Element {
    const [confirmationPrompt, setConfirmationPrompt] = useState<IConfirmationPrompt | null>(null);

    const showAsyncCallback = useCallback(
        (options?: IConfirmationPromptOptions) => new Promise<void>((resolve, reject) => {
            setConfirmationPrompt((previousConfirmationPrompt) => {
                previousConfirmationPrompt?.dismiss();

                return {
                    options,

                    confirm() {
                        try {
                            const onConfirm = options?.onConfirm;
                            onConfirm && onConfirm();

                            setConfirmationPrompt(null);
                            resolve();
                        }
                        catch (error) {
                            reject(error);
                        }
                    },

                    dismiss() {
                        try {
                            const onDismiss = options?.onDismiss;
                            onDismiss && onDismiss();

                            setConfirmationPrompt(null);
                            resolve();
                        }
                        catch (error) {
                            reject(error);
                        }
                    }
                };
            });
        }),
        [setConfirmationPrompt]
    );

    const confirmationPromptContext = useMemo<IConfirmationPromptContext>(
        () => ({
            confirmationPrompt,

            showAsync: showAsyncCallback
        }),
        [confirmationPrompt, showAsyncCallback]
    );

    return (
        <ConfirmationPromptContext.Provider
            value={confirmationPromptContext}
            children={children}
        />
    );
}