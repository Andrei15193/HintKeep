import { IEventHandler, INotifyPropertyChanged } from '../../events';
import { ViewModel } from './view-model';
import { FormFieldViewModel } from './form-field-view-model';

export type FieldChangedCallback<TValue> = (field: FormFieldViewModel<TValue>, changedProperties: readonly string[]) => void;
export type FieldCollectionChangedCallback = (fieldCollection: FormFieldCollectionViewModel, changedProperties: readonly string[]) => void;

interface IObservableSubscriptionPair {
    readonly propertyChangedNotifier: INotifyPropertyChanged,
    readonly eventHandler: IEventHandler<readonly string[]>;
}

export class FormFieldCollectionViewModel extends ViewModel {
    private readonly _fields: FormFieldViewModel<any>[] = [];
    private readonly _fieldCollections: FormFieldCollectionViewModel[] = [];
    private readonly _substriptions: IObservableSubscriptionPair[] = [];

    public constructor(...stores: readonly INotifyPropertyChanged[]) {
        super(...stores);
    }

    public get fields(): readonly FormFieldViewModel<any>[] {
        return this._fields;
    }

    protected get allFields(): readonly FormFieldViewModel<any>[] {
        let index = 0;
        const allFields: FormFieldViewModel<any>[] = [];
        const allFieldCollections: FormFieldCollectionViewModel[] = [this];
        do {
            const currentFieldCollection = allFieldCollections[index];
            currentFieldCollection.fields.forEach(field => allFields.push(field));
            currentFieldCollection.fieldCollections.forEach(fieldCollection => allFieldCollections.push(fieldCollection));
            index++;
        } while (index < allFieldCollections.length);

        return allFields;
    }

    protected get fieldCollections(): readonly FormFieldCollectionViewModel[] {
        return this._fieldCollections;
    }

    protected get allFieldCollections(): readonly FormFieldCollectionViewModel[] {
        let index = 0;
        const allFieldCollections: FormFieldCollectionViewModel[] = [this];
        do {
            const currentFieldCollection = allFieldCollections[index];
            currentFieldCollection.fieldCollections.forEach(fieldCollection => allFieldCollections.push(fieldCollection));
            index++;
        } while (index < allFieldCollections.length);

        return allFieldCollections;
    }

    public get isValid(): boolean {
        return this.fields.every(field => field.isValid) && this.fieldCollections.every(fieldCollection => fieldCollection.isValid);
    }

    public get isInvalid(): boolean {
        return this.fields.some(field => field.isInvalid) || this.fieldCollections.some(fieldCollection => fieldCollection.isInvalid);
    }

    protected addField<TValue>(initialValue: TValue, ...fieldChangedCallbacks: readonly (FieldChangedCallback<TValue> | undefined)[]): FormFieldViewModel<TValue> {
        const field = new FormFieldViewModel<TValue>(initialValue);
        this._fields.push(field);

        const fieldChangedCallback = this.__FormFieldCollectionViewModel_combineCallbkacs<FieldChangedCallback<TValue>>(...fieldChangedCallbacks, this.__FormFieldCollectionViewModel_fieldChanged);
        if (fieldChangedCallback) {
            const propertyChangedEventHandler: IEventHandler<readonly string[]> = { handle: fieldChangedCallback };
            field.propertyChanged.subscribe(propertyChangedEventHandler);
            this._substriptions.push({ propertyChangedNotifier: field, eventHandler: propertyChangedEventHandler });
            fieldChangedCallback(field, ['value']);
        }

        this.notifyPropertyChanged('fields');
        return field;
    }

    protected removeField<TValue>(field: FormFieldViewModel<TValue>): void {
        this.__FormFieldCollectionViewModel_removeSubscription(field);
        const indexToRemove = this._fields.indexOf(field);
        if (indexToRemove >= 0)
            this._fields.splice(indexToRemove, 1);

        this.notifyPropertyChanged('fields');
    }

    protected addFieldCollection(...fieldCollectionChangedCallbacks: readonly (FieldCollectionChangedCallback | undefined)[]): FormFieldCollectionViewModel {
        const fieldCollection = new FormFieldCollectionViewModel();
        this._fieldCollections.push(fieldCollection);

        const fieldCollectionChangedCallback = this.__FormFieldCollectionViewModel_combineCallbkacs<FieldCollectionChangedCallback>(...fieldCollectionChangedCallbacks, this.__FormFieldCollectionViewModel_fieldCollectionChanged);
        if (fieldCollectionChangedCallback) {
            const propertyChangedEventHandler: IEventHandler<readonly string[]> = { handle: fieldCollectionChangedCallback };
            fieldCollection.propertyChanged.subscribe(propertyChangedEventHandler);
            this._substriptions.push({ propertyChangedNotifier: fieldCollection, eventHandler: propertyChangedEventHandler });
            fieldCollectionChangedCallback(fieldCollection, ['value']);
        }

        this.notifyPropertyChanged('fieldCollections');
        return fieldCollection;
    }

    protected removeFieldCollection(fieldCollection: FormFieldCollectionViewModel): void {
        this.__FormFieldCollectionViewModel_removeSubscription(fieldCollection);
        const indexToRemove = this._fieldCollections.indexOf(fieldCollection);
        if (indexToRemove >= 0)
            this._fieldCollections.splice(indexToRemove, 1);

        this.notifyPropertyChanged('fieldCollections');
    }

    private __FormFieldCollectionViewModel_fieldChanged(field: FormFieldViewModel<any>, changedProperties: readonly string[]): void {
        if (changedProperties.includes('isValid') || changedProperties.includes('isInvalid'))
            this.notifyPropertyChanged('isValid', 'isInvald');
    }

    private __FormFieldCollectionViewModel_fieldCollectionChanged(fieldCollection: FormFieldCollectionViewModel, changedProperties: readonly string[]): void {
        if (changedProperties.includes('isValid') || changedProperties.includes('isInvalid'))
            this.notifyPropertyChanged('isValid', 'isInvald');
    }

    private __FormFieldCollectionViewModel_removeSubscription(propertyChangedNotifier: INotifyPropertyChanged): void {
        const indexToRemove = this._substriptions.findIndex(({ propertyChangedNotifier: subscribedPropertyChangedNotifier }) => subscribedPropertyChangedNotifier === propertyChangedNotifier);
        if (indexToRemove >= 0)
            this
                ._substriptions
                .splice(indexToRemove, 1)
                .forEach(({ propertyChangedNotifier, eventHandler }) => propertyChangedNotifier.propertyChanged.unsubscribe(eventHandler));
    }

    private __FormFieldCollectionViewModel_combineCallbkacs<TCallback extends (...args: any[]) => void>(...callbacks: readonly (TCallback | undefined)[]): TCallback | undefined {
        const actualCallbacks = callbacks.filter((callback): callback is TCallback => callback !== undefined).map(callback => callback.bind(this) as TCallback);
        switch (actualCallbacks ? actualCallbacks.length : 0) {
            case 0:
                return undefined;

            case 1:
                return actualCallbacks[0];

            default:
                return ((...args: any[]) => actualCallbacks.forEach(callback => callback(...args))) as TCallback;
        }
    }
}