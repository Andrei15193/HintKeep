import type { HintKeepFormField } from "../../ViewModels";
import React, { type PropsWithChildren } from "react";
import { useViewModel } from "react-model-view-viewmodel";

export interface ILabelProps {
    readonly field: HintKeepFormField<unknown>;
}

export function Label(props: PropsWithChildren<ILabelProps>): React.JSX.Element {
    const {
        field,
        children = field.label + (field.isRequired ? "*" : "")
    } = props;

    useViewModel(field);

    return (
        <label htmlFor={field.name}>
            {children}
        </label>
    );
}