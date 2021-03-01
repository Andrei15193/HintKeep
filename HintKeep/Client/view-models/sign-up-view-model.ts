import type { IObservable } from '../observer';
import type { IFormField } from './core';
import { DispatchObservable } from '../observer';
import { FormViewModel, FormField } from './core';

export class SignUpViewModel extends FormViewModel {
    private readonly _submittedEvent: DispatchObservable;

    constructor() {
        super();
        this._submittedEvent = new DispatchObservable();
        this.register(
            this.email = new FormField<string>(''),
            this.password = new FormField<string>('')
        );
    }

    public readonly email: IFormField<string>;

    public readonly password: IFormField<string>;

    public get submittedEvent(): IObservable {
        return this._submittedEvent;
    }

    public submitAsync(): Promise<void> {
        return this.handleAsync(async () => {
            this.touchAllFields();
            if (this.isValid) {
                await this.delay(3000);
                this._submittedEvent.dispatch(undefined);
            }
        });
    }

    protected fieldChanged(field: FormField<string>) {
        if (field.value.length === 0)
            field.errors = ["Required"];
        else
            field.errors = [];
        super.notifyChanged();
    }
}