import React from "react";
import { type ICheckboxProps, Checkbox } from "../../Core/Checkbox";
import { useFormContext } from "../../FormContext";
import { useFormFieldContext } from "../FormField/UseFormFieldContext";

export interface IFormFieldCheckboxProps extends Omit<ICheckboxProps, "field"> {
}

export function FormFieldCheckbox(props: IFormFieldCheckboxProps): React.JSX.Element {
    const { disabled } = useFormContext();
    const { field } = useFormFieldContext<boolean>();

    return (
        <Checkbox
            disabled={disabled}
            {...props}
            field={field}
        />
    );
}