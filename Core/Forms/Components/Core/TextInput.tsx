import type { HintKeepFormField } from "../../ViewModels";
import React, { type ChangeEvent, useCallback } from "react";
import { useViewModel } from "react-model-view-viewmodel";
import { InputContainer } from "./InputContainer";
import { shouldShowError } from "./ShouldShowError";

export interface ITextInputProps {
    readonly field: HintKeepFormField<string>;
    readonly disabled?: boolean;
    readonly placeholder?: string;

    readonly type?: "email" | "search" | "tel" | "text" | "url" | "password";
}

export function TextInput({ field, type = "text", placeholder, disabled }: ITextInputProps): React.JSX.Element {
    const { name, value, error } = useViewModel(field);
    const showError = shouldShowError(field);

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
        <InputContainer
            showError={showError}
            error={error}
        >
            <input
                id={name}
                name={name}
                value={value}
                type={type}
                onBlur={onBlurCallback}
                onChange={onChangedCallback}
                className={showError ? "invalid" : ""}
                placeholder={placeholder}
                disabled={disabled}
            />
        </InputContainer>
    );
}