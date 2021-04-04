import type { InputHTMLAttributes } from 'react'
import type { IFormField } from '../../../view-models/core';
import React, { useContext } from 'react';
import classnames from 'classnames';
import { watchViewModel } from '../../view-model-hooks';
import { getValidationClasses } from './get-validation-classes';
import { I18nContext } from '../../i18n';

import Style from '../../style.scss';

export interface IInputProps extends InputHTMLAttributes<HTMLInputElement> {
    field: IFormField
}

export function Input({ field, placeholder, ...inputProps }: IInputProps): JSX.Element {
    const messageResolver = useContext(I18nContext);
    watchViewModel(field, ['value']);

    return (
        <input
            className={classnames(Style.formControl, getValidationClasses(field))}
            onFocus={() => field.isTouched = true}
            value={field.value}
            onChange={ev => field.value = ev.target.value}
            placeholder={placeholder ? messageResolver.resolve(placeholder) : undefined}
            autoComplete="off"
            {...inputProps} />
    );
}