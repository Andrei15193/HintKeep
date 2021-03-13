import type { ConsumerProps } from 'react';
import type { IMessageResolver } from './i18n-context';
import React from 'react';
import { I18nContext } from './i18n-context';

export function I18nConsumer(props: ConsumerProps<IMessageResolver>): JSX.Element {
    return <I18nContext.Consumer {...props} />;
}