import type { AxiosResponse } from 'axios';
import type { IPreconditionFailedResponseData, IRequestData, IResponseData, IUnprocessableEntityResponseData } from '../api/users/confirmations/post';
import { FormViewModel, FormField } from './core';
import { Axios } from '../services';
import { DispatchEvent, IEvent } from '../events';

export class UserConfirmationViewModel extends FormViewModel {
    private readonly _submittedEvent: DispatchEvent;
    private readonly _email: FormField<string>;
    private readonly _confirmationToken: FormField<string>;

    constructor() {
        super(Axios);
        this._submittedEvent = new DispatchEvent();
        this.register(
            this._email = new FormField<string>(''),
            this._confirmationToken = new FormField<string>('')
        );
    }

    public get email(): Readonly<FormField<string>> {
        return this._email;
    }

    public get confirmationToken(): Readonly<FormField<string>> {
        return this._confirmationToken;
    }

    public get submittedEvent(): IEvent {
        return this._submittedEvent;
    }

    public async submitAsync(): Promise<void> {
        this.touchAllFields();
        if (this.isValid)
            await this
                .post<IRequestData>('/api/users/confirmations', {
                    email: this.email.value,
                    confirmationToken: this.confirmationToken.value
                })
                .on(201, (_: AxiosResponse<IResponseData>) => {
                    this._submittedEvent.dispatch(this);
                })
                .on(412, (_: AxiosResponse<IPreconditionFailedResponseData>) => {
                    this._confirmationToken.errors = ['validation.errors.invalidSignUpConfirmationToken'];
                })
                .on(422, ({ data: { email: emailErrors, confirmationToken: confirmationTokenErrors } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                    this._email.errors = emailErrors;
                    this._confirmationToken.errors = confirmationTokenErrors;
                })
                .sendAsync();
    }

    protected fieldChanged(field: FormField<string>, changedProperties: readonly string[]): void {
        if ((changedProperties.includes('value') || changedProperties.includes('isTouched')))
            field.errors = field.value?.length ? [] : ['validation.errors.required'];
        super.fieldChanged(field, changedProperties);
    }
}