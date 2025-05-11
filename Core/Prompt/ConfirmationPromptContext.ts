import { type SyntheticEvent, createContext, useCallback, useContext } from "react";

/**
 * Expected global context, set at app root.
 */
export const ConfirmationPromptContext = createContext<IConfirmationPromptContext>(null!);

export interface IConfirmationPromptContext {
    readonly confirmationPrompt: IConfirmationPrompt | null;

    showAsync(options?: IConfirmationPromptOptions): Promise<void>;
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

export interface IConfirmationPromptEvents {
    onConfirm?(): void;
    onDismiss?(): void;
}

export type ShowConfirmationPromptAsyncCallback = (event?: SyntheticEvent, confirmationPromptEvents?: IConfirmationPromptEvents) => Promise<void>;

export function useShowConfirmationPrompt(options: IConfirmationPromptOptions = {}): ShowConfirmationPromptAsyncCallback {
    const { showAsync } = useContext(ConfirmationPromptContext);

    const {
        message,
        confirmButtonLabel,
        dismissButtonLabel,
        onConfirm,
        onDismiss
    } = options;

    return useCallback(
        (event, confirmationPromptEvents) => {
            event?.preventDefault();

            const onInstanceConfirm = confirmationPromptEvents?.onConfirm;
            const onInstanceDismiss = confirmationPromptEvents?.onDismiss;

            return showAsync({
                message,
                confirmButtonLabel,
                dismissButtonLabel,
                onConfirm() {
                    onInstanceConfirm && onInstanceConfirm();
                    onConfirm && onConfirm();
                },
                onDismiss() {
                    onInstanceDismiss && onInstanceDismiss();
                    onDismiss && onDismiss();
                }
            });
        },
        [message, confirmButtonLabel, dismissButtonLabel, onConfirm, onDismiss, showAsync]
    );
}