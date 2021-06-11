import type { TextareaHTMLAttributes } from 'react';
import type { IFormFieldViewModel } from 'react-model-view-viewmodel';
import React, { useContext } from 'react';
import { watchViewModel } from 'react-model-view-viewmodel';
import classnames from 'classnames';
import { I18nContext } from '../../i18n';
import { getValidationClasses } from './get-validation-classes';

import Style from '../../style.scss';

export interface ITextAreaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
    field: IFormFieldViewModel<string | undefined>
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