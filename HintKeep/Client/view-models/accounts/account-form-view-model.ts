import { FormFieldViewModel, FormFieldCollectionViewModel, registerValidators } from 'react-model-view-viewmodel';
import { required } from '../validation';

export class AccountFormViewModel extends FormFieldCollectionViewModel {
    public constructor() {
        super();
        registerValidators(this.name = this.addField('name', ''), [required]);
        registerValidators(this.hint = this.addField('hint', ''), [required]);
        this.isPinned = this.addField('isPinned', false);
        this.notes = this.addField('notes', '');

        this.fields.forEach(field => field.propertiesChanged.subscribe({ handle: this._fieldChanged }));
    }

    public readonly name: FormFieldViewModel<string>;
    public readonly hint: FormFieldViewModel<string>;
    public readonly isPinned: FormFieldViewModel<boolean>;
    public readonly notes: FormFieldViewModel<string>;

    public get areAllFieldsTouched(): boolean {
        return this.fields.every(field => field.isTouched);
    }

    private _fieldChanged = (field: FormFieldViewModel<any>, changedProperties: readonly string[]): void => {
        if (changedProperties.includes('isTouched'))
            this.notifyPropertiesChanged('areAllFieldsTouched');
    };
}