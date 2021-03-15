import type { AxiosResponse } from 'axios';
import type { IFormField } from '../core';
import type { IUnauthorizedResponseData, IRequestData, IResponseData, IUnprocessableEntityResponseData } from '../../api/users/sessions/post';
import { Axios } from '../../services';
import { userStore } from '../../stores';
import { FormField, FormViewModel } from '../core';

export class LoginViewModel extends FormViewModel {
    private readonly _email: FormField<string>;
    private readonly _password: FormField<string>;

    public constructor() {
        super(Axios, userStore);
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

    public async loginAsync(): Promise<void> {
        this.touchAllFields();
        if (this.isValid) {
            await this
                .post<IRequestData>('/api/users/sessions', {
                    email: this.email.value,
                    password: this.password.value
                })
                .on(201, ({ data: { jsonWebToken } }: AxiosResponse<IResponseData>) => {
                    userStore.startSession(jsonWebToken);
                })
                .on(401, (_: AxiosResponse<IUnauthorizedResponseData>) => {
                    this._email.errors = ['validation.errors.invalidCredentials'];
                })
                .on(422, ({ data: { email: emailErrors, password: passwordErrors } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                    this._email.errors = emailErrors;
                    this._password.errors = passwordErrors;
                })
                .sendAsync();
        }
    }

    public logOut(): void {
        userStore.completeSession();
    }

    protected fieldChanged(field: FormField<string>, changedProperties: readonly string[]): void {
        if ((changedProperties.includes('value') || changedProperties.includes('isTouched'))) {
            field.errors = field.value?.length ? [] : ['validation.errors.required'];

            if (field == this._password && this._email.value.length > 0)
                this._email.errors = [];
        }
        super.fieldChanged(field, changedProperties);
    }
}