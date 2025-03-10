import type { HintKeepFormField } from "../../Forms";
import React, { type ChangeEvent, useCallback } from "react";
import { useViewModel } from "react-model-view-viewmodel";

export interface ITextInputProps {
    readonly field: HintKeepFormField<string>;

    readonly type?: "email" | "search" | "tel" | "text" | "url";
}

export function TextInput({ field, type = "text" }: ITextInputProps): React.JSX.Element {
    useViewModel(field);

    const onChangedCallback = useCallback(
        (event: ChangeEvent<HTMLInputElement>) => {
            field.value = event.target.value;
        },
        [field]
    );

    const onFocusCallback = useCallback(
        () => {
            field.wasTouched = true;
        },
        [field]
    );

    return (
        <input
            autoComplete="off"
            autoCorrect="off"
            id={field.name}
            name={field.name}
            value={field.value}
            type={type}
            onFocus={onFocusCallback}
            onChange={onChangedCallback}
            className={field.wasTouched && field.isInvalid ? "invalid" : ""}
        />
    );
}