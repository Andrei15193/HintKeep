import React, { type PropsWithChildren } from "react";
import { type ILabelProps, Label } from "../../Core/Label";
import { useFormFieldContext } from "../FormField";

export interface IFormFieldLabelProps extends Omit<ILabelProps, "field"> {
    readonly name?: string;
    readonly label?: string;
}

export function FormFieldLabel(props: PropsWithChildren<IFormFieldLabelProps>): React.JSX.Element {
    const { field } = useFormFieldContext();

    return (
        <Label
            {...props}
            field={field}
        />
    );
}