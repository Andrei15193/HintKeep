import type { HintKeepFormField } from "../../Forms";
import React, { type ChangeEvent, useCallback } from "react";
import { useViewModel } from "react-model-view-viewmodel";

export interface ITextAreaProps {
    readonly field: HintKeepFormField<string>;
    readonly isInvalid?: boolean;
    readonly disabled?: boolean;
}

export function TextArea({ field, isInvalid, disabled }: ITextAreaProps): React.JSX.Element {
    useViewModel(field);

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
        <>
            <textarea
                autoComplete="off"
                id={field.name}
                name={field.name}
                value={field.value}
                onBlur={onBlurCallback}
                onChange={onChangedCallback}
                className={field.wasTouched && (isInvalid || field.isInvalid) ? "invalid" : ""}
                disabled={disabled}
            />
            {field.wasTouched && (isInvalid || field.isInvalid) ? field.error : null}
        </>
    );
}