import { Store } from './store';

export interface ITranslations {
    'en-GB': IMessages;
}

export interface IMessages {
    [key: string]: string | IMessages;
}

export class TranslationStore extends Store {
    private readonly _messages: ITranslations;
    private _locale: keyof ITranslations;

    public constructor(messages: ITranslations, locale: keyof ITranslations) {
        super();
        this._messages = messages;
        this._locale = locale;
    }

    public get locale(): keyof ITranslations {
        return this._locale;
    }

    public set locale(value: keyof ITranslations) {
        if (this._locale !== value) {
            this._locale = value;
            this.notifyPropertyChanged('locale');
        }
    }

    public getMessage(id: string): string {
        const translatedMessage = id.split('.').reduce<IMessages | string | null>(
            (messageCollection, key) => {
                if (isMessageCollection(messageCollection))
                    if (key in messageCollection)
                        return messageCollection[key];
                    else {
                        console.error(`Cannot find key '${key}' in translation message id '${id}'.`);
                        return null;
                    }
                else
                    return null;
            },
            this._messages[this._locale]
        );

        if (typeof translatedMessage === 'string')
            return translatedMessage;
        else if (translatedMessage !== null) {
            console.error(`Expected string for translation message id '${id}' but found ${JSON.stringify(translatedMessage, null, '    ')}`)
            return id;
        }
        else
            return id;
    }
}

function isMessageCollection(obj: any): obj is IMessages {
    return obj && typeof (obj) === 'object';
}