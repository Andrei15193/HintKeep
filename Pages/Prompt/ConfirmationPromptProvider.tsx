import React, { type PropsWithChildren, useCallback, useMemo, useState } from "react";
import { type IConfirmationPrompt, type IConfirmationPromptContext, ConfirmationPromptContext } from "./ConfirmationPromptContext";

export function ConfirmationPromptProvider({ children }: PropsWithChildren<{}>): React.JSX.Element {
    const [prompt, setPrompt] = useState<IConfirmationPrompt | null>(null);

    const dismissCallback = useCallback(() => setPrompt(null), [setPrompt]);

    const confirmationPromptContext = useMemo<IConfirmationPromptContext>(
        () => ({
            prompt,
            show: setPrompt,
            dismiss: dismissCallback
        }),
        [prompt, dismissCallback, setPrompt]
    );

    return (
        <ConfirmationPromptContext.Provider
            value={confirmationPromptContext}
            children={children}
        />
    );
}