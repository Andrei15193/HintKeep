import type { TextareaHTMLAttributes } from 'react';
import type { HintKeepFormField } from '../../view-models/forms';
import { useContext } from 'react';
import classnames from 'classnames';
import { I18nContext } from '../i18n';
import { getValidationClasses } from './get-validation-classes';
import { useViewModel } from 'react-model-view-viewmodel';

import Style from '../style.scss';

export interface ITextAreaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
    field: HintKeepFormField<string | undefined>
}

export function TextArea({ field, placeholder, ...textAreaProps }: ITextAreaProps): JSX.Element {
    const messageResolver = useContext(I18nContext);
    useViewModel(field);

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