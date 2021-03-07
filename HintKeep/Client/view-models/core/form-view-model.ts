import type { AxiosInstance } from 'axios';
import type { IEventHandler, INotifyPropertyChanged } from '../../events';
import { ViewModel } from './view-model';
import { ApiViewModel } from './api-view-model';

export class FormViewModel extends ApiViewModel {
    private readonly _fieldPropertyChangedEventHandler: IEventHandler<readonly string[]>;
    protected readonly _registeredFields: IFormField[];

    constructor(axios: AxiosInstance, ...stores: readonly INotifyPropertyChanged[]) {
        super(axios, ...stores);
        this._fieldPropertyChangedEventHandler = { handle: this.fieldChanged.bind(this) };
        this._registeredFields = [];
    }

    public get isValid(): boolean {
        return this._registeredFields.every(field => field.isValid);
    }

    public get isInvalid(): boolean {
        return this._registeredFields.some(field => field.isInvalid);
    }

    public get areAllFieldsTouched(): boolean {
        return this._registeredFields.every(field => field.isTouched);
    }

    protected get registeredFields(): readonly IFormField[] {
        return this._registeredFields;
    }

    public touchAllFields(): void {
        this._registeredFields.forEach(field => field.isTouched = true);
    }

    public untouchAllFields(): void {
        this._registeredFields.forEach(field => field.isTouched = false);
    }

    protected register(...fields: readonly IFormField[]): void {
        fields.forEach(field => {
            field.propertyChanged.subscribe(this._fieldPropertyChangedEventHandler);
            this._registeredFields.push(field);
        });
    }

    protected unregister(...fields: readonly IFormField[]): void {
        fields.forEach(field => {
            const indexToRemove = this._registeredFields.indexOf(field);
            if (indexToRemove >= 0) {
                field.propertyChanged.unsubscribe(this._fieldPropertyChangedEventHandler);
                this._registeredFields.splice(indexToRemove, 1);
            }
        });
    }

    protected fieldChanged(field: object, changedProperties: readonly string[]): void {
        this.notifyPropertyChanged('isValid', 'isInvalid', 'areAllFieldsTouched');
    }
}

export interface IFormField<TValue = any> extends INotifyPropertyChanged {
    isTouched: boolean;

    value: TValue;

    readonly isValid: boolean;

    readonly isInvalid: boolean;

    readonly errors: readonly string[];
}

export class FormField<TValue> extends ViewModel implements IFormField<TValue> {
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