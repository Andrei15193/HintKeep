import { type SyntheticEvent, createContext, useCallback, useContext } from "react";

export interface IConfirmationPromptContext {
    readonly confirmationPrompt: IConfirmationPrompt | null;

    show(options?: IConfirmationPromptOptions): void;
}

export interface IConfirmationPromptOptions {
    readonly message?: React.ReactNode;
    readonly confirmButtonLabel?: string;
    readonly dismissButtonLabel?: string;

    onConfirm?(): void;
    onDismiss?(): void;
}

export interface IConfirmationPrompt {
    readonly options?: IConfirmationPromptOptions;

    confirm(): void;
    dismiss(): void;
}

export type ShowConfirmationPromptCallback = (event?: SyntheticEvent) => void;

export function useShowConfirmationPrompt(options: IConfirmationPromptOptions = {}): ShowConfirmationPromptCallback {
    const { show } = useContext(ConfirmationPromptContext);

    const {
        message,
        confirmButtonLabel,
        dismissButtonLabel,
        onConfirm,
        onDismiss
    } = options;

    return useCallback(
        (event) => {
            event?.preventDefault();

            show({
                message,
                confirmButtonLabel,
                dismissButtonLabel,
                onConfirm,
                onDismiss
            });
        },
        [message, confirmButtonLabel, dismissButtonLabel, onConfirm, onDismiss, show]
    );
}

/**
 * Expected global context, set at app root.
 */
export const ConfirmationPromptContext = createContext<IConfirmationPromptContext>(null!);