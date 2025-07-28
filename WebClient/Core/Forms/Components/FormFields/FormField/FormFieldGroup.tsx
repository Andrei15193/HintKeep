import React, { type PropsWithChildren } from "react";

export interface IFormFieldGroupProps {
}

export function FormFieldGroup({ children }: PropsWithChildren<{}>): React.JSX.Element {
    return (
        <div className="form-field-group">
            {children}
        </div>
    );
}