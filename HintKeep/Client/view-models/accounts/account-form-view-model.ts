import { required } from '../validation';
import { HintKeepForm, HintKeepFormField } from '../forms';

export class AccountFormViewModel extends HintKeepForm {
    public constructor() {
        super();

        this.withFields(
            this.name = new HintKeepFormField<string>({
                name: 'name',
                initialValue: '',
                validators: [required]
            }),
            this.hint = new HintKeepFormField<string>({
                name: 'hint',
                initialValue: '',
                validators: [required]
            }),
            this.isPinned = new HintKeepFormField<boolean>({
                name: 'isPinned',
                initialValue: false
            }),
            this.notes = new HintKeepFormField<string>({
                name: 'notes',
                initialValue: ''
            })
        );
    }

    public readonly name: HintKeepFormField<string>;
    public readonly hint: HintKeepFormField<string>;
    public readonly isPinned: HintKeepFormField<boolean>;
    public readonly notes: HintKeepFormField<string>;
}