import type { AxiosResponse } from 'axios';
import type { IFormField } from '../core';
import type { IConflictResponseData, IRequestData, IResponseData, IUnprocessableEntityResponseData } from '../../api/users/post';
import { FormViewModel, FormField } from '../core';
import { Axios } from '../../services';
import { DispatchEvent, IEvent } from '../../events';

export class SignUpViewModel extends FormViewModel {
    private readonly _submittedEvent: DispatchEvent;
    private readonly _email: FormField<string>;
    private readonly _password: FormField<string>;

    constructor() {
        super(Axios);
        this._submittedEvent = new DispatchEvent();
        this.register(
            this._email = new FormField<string>(''),
            this._password = new FormField<string>('')
        );
    }

    public get email(): IFormField<string> {
        return this._email;
    }

    public get password(): IFormField<string> {
        return this._password;
    }

    public get submittedEvent(): IEvent {
        return this._submittedEvent;
    }

    public async submitAsync(): Promise<void> {
        this.touchAllFields();
        if (this.isValid) {
            await this
                .post<IRequestData>('/api/users', {
                    email: this.email.value,
                    password: this.password.value
                })
                .on(201, (_: AxiosResponse<IResponseData>) => {
                    this._submittedEvent.dispatch(this);
                })
                .on(409, (_: AxiosResponse<IConflictResponseData>) => {
                    this._email.errors = ['validation.errors.emailNotUnique'];
                })
                .on(422, ({ data: { email: emailErrors, password: passwordErrors } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                    this._email.errors = emailErrors;
                    this._password.errors = passwordErrors;
                })
                .sendAsync();
        }
    }

    protected fieldChanged(field: FormField<string>, changedProperties: readonly string[]): void {
        if (changedProperties.includes('value') || changedProperties.includes('isTouched'))
            field.errors = field.value?.length ? [] : ['validation.errors.required'];
        super.fieldChanged(field, changedProperties);
    }
}