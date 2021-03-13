import type { PropsWithChildren } from 'react';
import type { AxiosResponse } from 'axios';
import type { IMessages } from '../../translations/IMessages';
import type { IResponseData } from '../../api/users/preferences/preferred-languages/get';
import React, { useState, useEffect } from 'react';
import { enGB } from '../../translations';
import { Spinner } from '../loaders';
import { I18nContext, EmptyMessageResolver, MessageResolver } from './i18n-context';
import { Axios } from '../../services';

const defaultLocale = 'en-GB';
const messagesByLanguage: { [locale: string]: IMessages } = {
    'en-GB': enGB
};

export function I18nProvider({ children }: PropsWithChildren<{}>): JSX.Element {
    const [isLoading, setIsLoading] = useState(true);
    const [messageResolver, setMessageResolver] = useState(EmptyMessageResolver.Instance);

    useEffect(
        () => {
            setIsLoading(true);
            Axios
                .get('/api/users/preferences/preferred-languages')
                .then((response: AxiosResponse<IResponseData>) => {
                    const locale = response.status === 200
                        ? response.data.find(language => language in messagesByLanguage) || defaultLocale
                        : defaultLocale;
                    setMessageResolver(new MessageResolver(messagesByLanguage[locale]));
                })
                .then(() => setIsLoading(false));
        },
        []
    );

    return (
        <I18nContext.Provider value={messageResolver}>
            {isLoading ? <Spinner /> : children}
        </I18nContext.Provider>
    );
}