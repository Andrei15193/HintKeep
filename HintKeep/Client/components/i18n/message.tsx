import React from 'react';
import { I18nConsumer } from './i18n-consumer';

export interface IMessageProps {
    readonly id: string;
    readonly values?: any;
}

export function Message({ id, values }: IMessageProps): JSX.Element {
    return <I18nConsumer>{messageResolver => messageResolver.resolve(id, values)}</I18nConsumer>;
}