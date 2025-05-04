import type { HintKeepFormField } from "../../../ViewModels";
import { createContext } from "react";

export interface IFormFieldContext<TValue> {
    readonly field: HintKeepFormField<TValue>;
}

export const FormFieldContext = createContext<IFormFieldContext<unknown> | null>(null);