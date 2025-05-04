import type { HintKeepFormField } from "../../ViewModels";
import React, { type ChangeEvent, useCallback } from "react";
import { useViewModel } from "react-model-view-viewmodel";

export interface ICheckboxProps {
    readonly field: HintKeepFormField<boolean>;
    readonly disabled?: boolean;
}

export function Checkbox({ field, disabled }: ICheckboxProps): React.JSX.Element {
    useViewModel(field);

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
        <input
            id={field.name}
            name={field.name}
            checked={field.value}
            type="checkbox"
            onBlur={onBlurCallback}
            onChange={onChangedCallback}
            className={field.wasTouched && field.isInvalid ? "invalid" : ""}
            disabled={disabled}
        />
    );
}