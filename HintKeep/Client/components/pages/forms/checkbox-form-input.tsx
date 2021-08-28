import type { IInputProps } from './input';
import React from 'react';
import classnames from 'classnames';
import { watchViewModel } from 'react-model-view-viewmodel';
import { Input } from './input';
import { Message } from '../../i18n';

import Style from '../../style.scss';

export interface IFormCheckboxInputProps extends IInputProps {
    readonly label: string;
    readonly description?: string;
    readonly className?: string;
}

export function FormCheckboxInput({ label, description, field, id, className, type, ...inputProps }: IFormCheckboxInputProps): JSX.Element {
    watchViewModel(field, ['value', 'error']);

    return (
        <div className={classnames(className, Style.formCheck)}>
            <Input field={field} id={id} type="checkbox" className={Style.formCheckInput} checked={field.value} onChange={ev => field.value = ev.target.checked} {...inputProps} />
            <label htmlFor={id} className={Style.formCheckLabel}>
                <Message id={label} />
            </label>
            <div id={`${id}Feedback`} className={Style.invalidFeedback}>{field.error !== undefined && <Message id={field.error} />}</div>
            {description && <div className={Style.mt2}><Message id={description} /></div>}
        </div>
    );
}