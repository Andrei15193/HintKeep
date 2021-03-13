import React from 'react';
import { I18nConsumer } from './i18n-consumer';

export interface IMessageProps {
    readonly id: string
}

export function Message({ id }: IMessageProps): JSX.Element {
    return <I18nConsumer>{messageResolver => messageResolver.resolve(id)}</I18nConsumer>;
}