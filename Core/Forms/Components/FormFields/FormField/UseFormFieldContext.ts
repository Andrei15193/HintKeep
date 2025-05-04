import { useContext } from "react";
import { useViewModel } from "react-model-view-viewmodel";
import { type IFormFieldContext, FormFieldContext } from "./FormFieldContext";

export function useFormFieldContext<TValue = unknown>(): IFormFieldContext<TValue> {
    const formFieldContext = useContext(FormFieldContext) as IFormFieldContext<TValue> | null;
    if (formFieldContext === null)
        throw new Error("Expected the component to be wrapped by a FormField at any point in the hierarchy.");

    useViewModel(formFieldContext.field);

    return formFieldContext;
}