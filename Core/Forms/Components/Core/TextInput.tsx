import type { HintKeepFormField } from "../../ViewModels";
import React, { type ChangeEvent, useCallback } from "react";
import { useViewModel } from "react-model-view-viewmodel";

export interface ITextInputProps {
    readonly field: HintKeepFormField<string>;
    readonly disabled?: boolean;

    readonly type?: "email" | "search" | "tel" | "text" | "url" | "password";
}

export function TextInput({ field, type = "text", disabled }: ITextInputProps): React.JSX.Element {
    useViewModel(field);

    const onChangedCallback = useCallback(
        (event: ChangeEvent<HTMLInputElement>) => {
            field.value = event.target.value;
        },
        [field]
    );

    const onBlurCallback = useCallback(
        () => {
            field.wasTouched = true;
        },
        [field]
    );

    return (
        <input
            id={field.name}
            name={field.name}
            value={field.value}
            type={type}
            onBlur={onBlurCallback}
            onChange={onChangedCallback}
            className={field.wasTouched && field.isInvalid ? "invalid" : ""}
            disabled={disabled}
        />
    );
}