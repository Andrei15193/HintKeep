import type { PropsWithChildren } from 'react';
import type { IInputProps } from './input';
import classnames from 'classnames';
import { Input } from './input';
import { Message } from '../i18n';

import Style from '../style.scss';
import { useViewModel } from 'react-model-view-viewmodel';

export interface IFormCheckboxInputProps extends IInputProps {
    readonly label: string;
    readonly description?: string;
    readonly className?: string;
}

export function FormCheckboxInput({ label, description, field, id, className, type, children, ...inputProps }: PropsWithChildren<IFormCheckboxInputProps>): JSX.Element {
    useViewModel(field);

    return (
        <div className={classnames(className, Style.formCheck)}>
            <Input field={field} id={id} type="checkbox" className={Style.formCheckInput} checked={field.value} onChange={ev => field.value = ev.target.checked} {...inputProps} />
            <label htmlFor={id} className={Style.formCheckLabel}>
                <Message id={label} />
                {children}
            </label>
            <div id={`${id}Feedback`} className={Style.invalidFeedback}>{field.error !== null && <Message id={field.error} />}</div>
            {description && <div className={Style.mt2}><Message id={description} /></div>}
        </div>
    );
}