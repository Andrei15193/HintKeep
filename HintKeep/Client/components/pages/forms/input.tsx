import type { InputHTMLAttributes } from 'react'
import type { IFormField } from '../../../view-models/core';
import React from 'react';
import classnames from 'classnames';
import { getValidationClasses } from './get-validation-classes';

import Style from '../../style.scss';
import { WithViewModel } from '../../view-model-wrappers';
import { TranslationViewModel } from '../../../view-models/translation-view-model';

export interface IInputProps extends InputHTMLAttributes<HTMLInputElement> {
    field: IFormField
}

export function Input({ field, placeholder, ...inputProps }: IInputProps): JSX.Element {
    return (
        <WithViewModel viewModelType={TranslationViewModel}>{$vm =>
            <input
                className={classnames(Style.formControl, getValidationClasses(field))}
                onFocus={() => field.isTouched = true}
                value={field.value}
                onChange={ev => field.value = ev.target.value}
                placeholder={$vm.getMessage(placeholder)}
                {...inputProps} />
        }</WithViewModel>
    );
}