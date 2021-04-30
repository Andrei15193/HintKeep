import type { TextareaHTMLAttributes } from 'react';
import type { IFormField } from '../../../view-models/core';
import React, { useContext } from 'react';
import classnames from 'classnames';
import { I18nContext } from '../../i18n';
import { getValidationClasses } from './get-validation-classes';
import { watchViewModel } from '../../view-model-hooks';

import Style from '../../style.scss';

export interface ITextAreaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
    field: IFormField
}

export function TextArea({ field, placeholder, ...textAreaProps }: ITextAreaProps): JSX.Element {
    const messageResolver = useContext(I18nContext);
    watchViewModel(field, ['value']);

    return (
        <textarea
            className={classnames(Style.formControl, Style.resizeNone, getValidationClasses(field))}
            value={field.value}
            onChange={ev => field.value = ev.target.value}
            placeholder={placeholder ? messageResolver.resolve(placeholder) : undefined}
            autoComplete="off"
            {...textAreaProps} />
    );
}