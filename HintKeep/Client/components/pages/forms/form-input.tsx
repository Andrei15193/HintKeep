import type { IInputProps } from './input';
import React from 'react';
import { Input } from './input';
import { Message } from '../../i18n';

import Style from '../../style.scss';

export interface IFormInputProps extends IInputProps {
    readonly label: string;
    readonly description?: string;
    readonly className?: string;
}

export function FormInput({ label, description, field, id, className, ...inputProps }: IFormInputProps): JSX.Element {
    return (
        <div className={className}>
            <label htmlFor={id}><Message id={label} /></label>
            <Input field={field} id={id} {...inputProps} />
            <div id={`${id}Feedback`} className={Style.invalidFeedback}>{field.errors.map(error => <Message key={error} id={error} />)}</div>
            {description && <div className={Style.mt2}><Message id={description} /></div>}
        </div>
    );
}