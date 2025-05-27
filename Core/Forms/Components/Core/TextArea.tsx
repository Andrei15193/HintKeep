import type { HintKeepFormField } from "../../ViewModels";
import React, { type ChangeEvent, useCallback } from "react";
import { useViewModel } from "react-model-view-viewmodel";
import { InputContainer } from "./InputContainer";
import { shouldShowError } from "./ShouldShowError";

export interface ITextAreaProps {
    readonly field: HintKeepFormField<string>;
    readonly placeholder?: string;
    readonly disabled?: boolean;
}

export function TextArea({ field, placeholder, disabled }: ITextAreaProps): React.JSX.Element {
    const { name, value, error } = useViewModel(field);
    const showError = shouldShowError(field);

    const onChangedCallback = useCallback(
        (event: ChangeEvent<HTMLTextAreaElement>) => {
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
            <textarea
                id={name}
                name={name}
                value={value}
                onBlur={onBlurCallback}
                onChange={onChangedCallback}
                className={showError ? "invalid" : ""}
                placeholder={placeholder}
                disabled={disabled}
            />
        </InputContainer>
    );
}