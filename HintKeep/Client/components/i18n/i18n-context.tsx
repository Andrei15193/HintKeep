import type { IMessages } from '../../translations/IMessages';
import React from 'react';

export interface IMessageResolver {
    resolve(key: string): string;
}

export class EmptyMessageResolver implements IMessageResolver {
    public static readonly Instance: IMessageResolver = new EmptyMessageResolver();

    public resolve(key: string): string {
        console.error(`Cannot find translation key for '${key}', there is no translation context set.`);
        return key;
    }
}

export class MessageResolver implements IMessageResolver {
    private readonly _messages;

    public constructor(messages: IMessages) {
        this._messages = messages;
    }

    public resolve(key: string): string {
        if (key in this._messages)
            return this._messages[key];
        else {
            console.error(`Cannot find translation key for '${key}'.`);
            return key;
        }
    }
}

export const I18nContext = React.createContext<IMessageResolver>(new EmptyMessageResolver());