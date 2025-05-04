import React from "react";
import { type ITextAreaProps, TextArea } from "../../Core/TextArea";
import { type ITextInputProps, TextInput } from "../../Core/TextInput";
import { useFormContext } from "../../FormContext";
import { useFormFieldContext } from "../FormField/UseFormFieldContext";

export type IFormFieldTextInputProps =
    IFormFieldInlineTextInputProps
    | IFormFieldMultilineTextInputProps;

export interface IFormFieldInlineTextInputProps extends Omit<ITextInputProps, "field"> {
    readonly multiline?: false;
}

export interface IFormFieldMultilineTextInputProps extends Omit<ITextAreaProps, "field"> {
    readonly multiline: true;
}

export function FormFieldTextInput(props: IFormFieldTextInputProps): React.JSX.Element {
    const { disabled } = useFormContext();
    const { field } = useFormFieldContext<string>();

    if (props.multiline === true)
        return (
            <TextArea
                disabled={disabled}
                {...props}
                field={field}
            />
        );
    else
        return (
            <TextInput
                disabled={disabled}
                {...props}
                field={field}
            />
        );
}