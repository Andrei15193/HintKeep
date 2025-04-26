import { createContext, useContext } from "react";

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

export const ConfirmationPromptContext = createContext<IConfirmationPromptContext>(null!);

export function useConfirmationPrompt(): IConfirmationPromptContext {
    return useContext(ConfirmationPromptContext);
}