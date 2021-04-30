import type { ITextAreaProps } from './text-area';
import React from 'react';
import { watchViewModel } from '../../view-model-hooks';
import { TextArea } from './text-area';
import { Message } from '../../i18n';

import Style from '../../style.scss';

export interface IFormInputProps extends ITextAreaProps {
    readonly label: string;
    readonly description?: string;
    readonly className?: string;
}

export function FormTextArea({ label, description, field, id, className, ...textAreaProps }: IFormInputProps): JSX.Element {
    watchViewModel(field, ['errors']);

    return (
        <div className={className}>
            <label htmlFor={id}><Message id={label} /></label>
            <TextArea field={field} {...textAreaProps} />
            <div id={`${id}Feedback`} className={Style.invalidFeedback}>{field.errors.map(error => <Message key={error} id={error} />)}</div>
            {description && <div className={Style.mt2}><Message id={description} /></div>}
        </div>
    );
}