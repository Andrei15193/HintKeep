import type { InputHTMLAttributes } from 'react'
import type { IFormField } from '../../../view-models/core';
import React from 'react';
import classnames from 'classnames';
import { getValidationClasses } from './get-validation-classes';

import Style from '../../style.scss';

export interface IInputProps extends InputHTMLAttributes<HTMLInputElement> {
    field: IFormField
}

export function Input({ field, ...inputProps }: IInputProps): JSX.Element {
    return <input
        className={classnames(Style.formControl, getValidationClasses(field))}
        onFocus={() => field.isTouched = true}
        value={field.value}
        onChange={ev => field.value = ev.target.value}
        {...inputProps} />
}