import type { HintKeepFormField } from "../../Forms";
import React, { type ChangeEvent, useCallback, forwardRef } from "react";
import { useViewModel } from "react-model-view-viewmodel";

export interface ITextInputProps {
    readonly field: HintKeepFormField<string>;
    readonly isInvalid?: boolean;

    readonly type?: "email" | "search" | "tel" | "text" | "url" | "password";
}

export const TextInput = forwardRef<HTMLInputElement, ITextInputProps>(
    function TextInput({ field, type = "text", isInvalid }, ref): React.JSX.Element {
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
            <>
                <input
                    ref={ref}
                    autoComplete="off"
                    autoCorrect="off"
                    id={field.name}
                    name={field.name}
                    value={field.value}
                    type={type}
                    onBlur={onBlurCallback}
                    onChange={onChangedCallback}
                    className={field.wasTouched && (isInvalid || field.isInvalid) ? "invalid" : ""}
                />
                {field.wasTouched && (isInvalid || field.isInvalid) ? field.error : null}
            </>
        );
    }
);