import { createContext, useContext } from "react";

export interface IConfirmationPromptContext {
    readonly prompt: IConfirmationPrompt | null;

    show(prompt: IConfirmationPrompt): void;
    dismiss(): void;
}

export interface IConfirmationPrompt {
    readonly message: string;
    readonly confirmButtonLabel?: string;
    readonly dismissButtonLabel?: string;

    confirm(): void;
    dismiss(): void;
}

export const ConfirmationPromptContext = createContext<IConfirmationPromptContext | null>(null);

export function useConfirmationPrompt(): IConfirmationPrompt | null {
    return useContext(ConfirmationPromptContext)!.prompt;
}