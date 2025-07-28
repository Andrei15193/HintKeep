import type { HintKeepFormField } from "../../../ViewModels";
import React, { type PropsWithChildren, useMemo } from "react";

import { FormFieldContext, type IFormFieldContext } from "./FormFieldContext";

export interface IFormFieldProps<TValue> {
    readonly field: HintKeepFormField<TValue>;
}

export function FormField<TValue>({ field, children }: PropsWithChildren<IFormFieldProps<TValue>>): React.JSX.Element {
    const formFieldContext = useMemo<IFormFieldContext<TValue>>(() => ({ field }), [field]);

    return (
        <FormFieldContext value={formFieldContext}>
            <div className="form-field">
                {children}
            </div>
        </FormFieldContext>
    );
}