import type { ChangeEvent, InputHTMLAttributes } from 'react'
import { useCallback, useContext } from 'react';
import classnames from 'classnames';
import { getValidationClasses } from './get-validation-classes';
import { I18nContext } from '../i18n';
import { useViewModel } from 'react-model-view-viewmodel';
import type { HintKeepFormField } from '../../view-models/forms';

import Style from '../style.scss';

export interface IInputProps extends InputHTMLAttributes<HTMLInputElement> {
    readonly field: HintKeepFormField<any>
}

export function Input({ field, placeholder, ...inputProps }: IInputProps): JSX.Element {
    const messageResolver = useContext(I18nContext);
    useViewModel(field);

    const onFocusCallback = useCallback(
        () => {
            field.isTouched = true;
        },
        [field]
    );

    const onChangeCallback = useCallback(
        (event: ChangeEvent<HTMLInputElement>) => {
            field.value = event.target.value;
        },
        [field]
    )

    return (
        <input
            className={classnames(Style.formControl, getValidationClasses(field))}
            value={field.value}
            onFocus={onFocusCallback}
            onChange={onChangeCallback}
            placeholder={placeholder ? messageResolver.resolve(placeholder) : undefined}
            autoComplete="off"
            {...inputProps} />
    );
}