import type { HintKeepFormField } from "../../ViewModels";
import React, { type ChangeEvent, useCallback } from "react";
import { useViewModel } from "react-model-view-viewmodel";
import { InputContainer } from "./InputContainer";
import { shouldShowError } from "./ShouldShowError";

export interface ICheckboxProps {
    readonly field: HintKeepFormField<boolean>;
    readonly disabled?: boolean;
}

export function Checkbox({ field, disabled }: ICheckboxProps): React.JSX.Element {
    const { name, value, error } = useViewModel(field);
    const showError = shouldShowError(field);

    const onChangedCallback = useCallback(
        (event: ChangeEvent<HTMLInputElement>) => {
            field.value = event.target.checked;
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
                checked={value}
                type="checkbox"
                onBlur={onBlurCallback}
                onChange={onChangedCallback}
                className={showError ? "invalid" : ""}
                disabled={disabled}
            />
        </InputContainer>
    );
}