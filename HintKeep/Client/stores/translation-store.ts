import { Store } from './store';

export interface ITranslations {
    'en-GB': IMessages;
}

export interface IMessages {
    [key: string]: string;
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
        const message = this._messages[this._locale][id];
        if (!message) {
            console.error(`Cannot find translation message for id '${id}'.`);
            return id;
        }
        else
            return message;
    }
}