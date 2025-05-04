import { createContext, useContext } from "react";

export const FormContext = createContext<IFormContext | null>(null);

export interface IFormContext {
    readonly disabled: boolean;
}

export function useFormContext(): IFormContext {
    const formContext = useContext(FormContext);
    if (formContext === null)
        throw new Error("Expected the component to be wrapped by a Form at any point in the hierarchy.");

    return formContext;
}