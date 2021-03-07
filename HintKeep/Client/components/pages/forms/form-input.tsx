import type { IInputProps } from './input';
import React from 'react';
import { Input } from './input';

import Style from '../../style.scss';

export interface IFormInputProps extends IInputProps {
    label: string
    className?: string
}

export function FormInput({ label, field, id, className, ...inputProps }: IFormInputProps): JSX.Element {
    return (
        <div className={className}>
            <label htmlFor={id}>{label}</label>
            <Input field={field} id={id} {...inputProps} />
            <div id={`${id}Feedback`} className={Style.invalidFeedback}>{field.errors}</div>
        </div>
    );
}