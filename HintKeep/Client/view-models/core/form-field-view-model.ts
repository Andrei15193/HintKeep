import type { INotifyPropertyChanged } from '../../events';
import { ViewModel } from './view-model';

export interface IFormFieldViewModel<TValue> extends INotifyPropertyChanged {
    isTouched: boolean;

    value: TValue;

    readonly isValid: boolean;

    readonly isInvalid: boolean;

    readonly errors: readonly string[];
}

export class FormFieldViewModel<TValue> extends ViewModel implements IFormFieldViewModel<TValue> {
    private readonly _initialValue: TValue;
    private _value: TValue;
    private _isTouched: boolean;
    private _errors: readonly string[];

    public constructor(initalValue: TValue) {
        super();
        this._initialValue = initalValue;
        this._value = initalValue;
        this._isTouched = false;
        this._errors = [];
    }

    public get initialValue(): TValue {
        return this._initialValue;
    }

    public get value(): TValue {
        return this._value;
    }

    public set value(value: TValue) {
        if (this._value !== value) {
            this._value = value;
            this.notifyPropertyChanged('value');
        }
    }

    public get isTouched(): boolean {
        return this._isTouched;
    }

    public set isTouched(value: boolean) {
        if (this.isTouched !== value) {
            this._isTouched = value;
            this.notifyPropertyChanged('isTouched');
        }
    }

    public get isValid(): boolean {
        return this._errors.length === 0;
    }

    public get isInvalid(): boolean {
        return this._errors.length > 0;
    }

    public get errors(): readonly string[] {
        return this._errors;
    }

    public set errors(value: readonly string[]) {
        const errorsValue = value || [];
        if (this._errors !== errorsValue
            && (this._errors.length !== errorsValue.length
                || this._errors.some(error => !errorsValue.includes(error))
                || errorsValue.some(error => !this._errors.includes(error)))) {
            this._errors = errorsValue;
            this.notifyPropertyChanged('errors', 'isValid', 'isInvalid');
        }
    }
}