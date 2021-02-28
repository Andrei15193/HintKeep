import type { IObservable, IObserver } from '../../observer';
import { ViewModel } from './view-model';
import { AsyncViewModel } from './async-view-model';

export class FormViewModel extends AsyncViewModel {
    private readonly _fieldObserver: IObserver;
    protected readonly _registeredFields: IFormField[];

    constructor(...aggregateObservables: IObservable[]) {
        super(...aggregateObservables);
        this._fieldObserver = {
            notifyChanged: (subject: IFormField) => this.fieldChanged(subject)
        };
        this._registeredFields = [];
    }

    public get isValid(): boolean {
        return this._registeredFields.every(field => field.isValid);
    }

    public get isInvalid(): boolean {
        return this._registeredFields.some(field => field.isInvalid);
    }

    protected get registeredFields(): Readonly<IFormField[]> {
        return this._registeredFields;
    }

    public touchAllFields(): void {
        this._registeredFields.forEach(field => field.isTouched = true);
    }

    public untouchAllFields(): void {
        this._registeredFields.forEach(field => field.isTouched = false);
    }

    protected register(...fields: Readonly<IFormField[]>): void {
        fields.forEach(field => {
            field.subscribe(this._fieldObserver);
            this._registeredFields.push(field);
        });
    }

    protected unregister(...fields: Readonly<IFormField[]>): void {
        fields.forEach(field => {
            field.unsubscribe(this._fieldObserver);
            const indexToRemove = this._registeredFields.indexOf(field);
            if (indexToRemove >= 0)
                this._registeredFields.splice(indexToRemove, 1);
        });
    }

    protected fieldChanged(field: IFormField): void {
        this.notifyChanged();
    }
}

export interface IFormField<TValue = any> extends IObservable {
    isTouched: boolean;

    value: TValue;

    readonly isValid: boolean;

    readonly isInvalid: boolean;

    readonly errors: Readonly<string[]>;
}

export class FormField<TValue> extends ViewModel implements IFormField<TValue> {
    private readonly _initialValue: TValue;
    private _value: TValue;
    private _isTouched: boolean;
    private _errors: Readonly<string[]>;

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

    public get hasValueChanged(): boolean {
        return this._initialValue !== this._value;
    }

    public set value(value: TValue) {
        if (this._value !== value) {
            this._value = value;
            this.notifyChanged();
        }
    }

    public get isTouched(): boolean {
        return this._isTouched;
    }

    public set isTouched(value: boolean) {
        if (this.isTouched !== value) {
            this._isTouched = value;
            this.notifyChanged();
        }
    }

    public get isValid(): boolean {
        return this._errors.length === 0;
    }

    public get isInvalid(): boolean {
        return this._errors.length > 0;
    }

    public get errors(): Readonly<string[]> {
        return this._errors;
    }

    public set errors(value: Readonly<string[]>) {
        if (this._errors !== value
            && (this._errors.length !== value.length
                || this._errors.some(error => !value.includes(error))
                || value.some(error => !this._errors.includes(error)))) {
            this._errors = value;
            this.notifyChanged();
        }
    }
}