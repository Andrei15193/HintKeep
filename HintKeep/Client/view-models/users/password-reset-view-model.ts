import type { AxiosResponse } from "axios";
import type { IEvent } from "react-model-view-viewmodel";
import type { INotFoundResponseData, IRequestData, IResponseData, IUnprocessableEntityResponseData } from "../../api/users/passwords/post";
import { DispatchEvent, FormFieldCollectionViewModel, FormFieldViewModel, registerValidators } from "react-model-view-viewmodel";
import { ApiViewModel } from "../api-view-model";
import { required } from "../validation";

export class PasswordResetViewModel extends ApiViewModel {
    private readonly _passwordReset: DispatchEvent = new DispatchEvent();

    public readonly form: PasswordResetFormViewModel = new PasswordResetFormViewModel();

    public get passwordReset(): IEvent {
        return this._passwordReset;
    }

    public async resetPasswordAsync(): Promise<void> {
        this.form.fields.forEach(field => field.isTouched = true);
        if (this.form.isValid)
            await this
                .post<IRequestData>('/api/users/passwords', {
                    email: this.form.email.value,
                    token: this.form.token.value,
                    password: this.form.password.value
                })
                .on(201, (_: AxiosResponse<IResponseData>) => {
                    this._passwordReset.dispatch(this);
                })
                .on(404, (_: AxiosResponse<INotFoundResponseData>) => {
                    this.form.token.error = 'errors.passwordReset.tokenExpired';
                })
                .on(422, ({ data: { email: emailErrors = [], token: tokenErrors = [], password: passwordErrors = [] } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                    this.form.email.error = emailErrors[0];
                    this.form.token.error = tokenErrors[0];
                    this.form.password.error = passwordErrors[0];
                })
                .sendAsync();
    }
}

class PasswordResetFormViewModel extends FormFieldCollectionViewModel {
    public constructor() {
        super();
        registerValidators(this.email = this.addField('email', ''), [required]);
        registerValidators(this.token = this.addField('token', ''), [required]);
        registerValidators(this.password = this.addField('password', ''), [required]);
        registerValidators(
            {
                target: this.passwordConfirmation = this.addField('passwordConfirmation', ''),
                triggers: [this.password]
            },
            [
                required,
                () => this.passwordConfirmation.value !== this.password.value ? 'validation.errors.passwordsDoNotMatch' : undefined
            ]
        );

        this.fields.forEach(field => field.propertiesChanged.subscribe({ handle: this._fieldChanged }));
    }

    public readonly email: FormFieldViewModel<string>;

    public readonly token: FormFieldViewModel<string>;

    public readonly password: FormFieldViewModel<string>;

    public readonly passwordConfirmation: FormFieldViewModel<string>;

    public get areAllFieldsTouched(): boolean {
        return this.fields.every(field => field.isTouched);
    }

    private _fieldChanged = (field: FormFieldViewModel<any>, changedProperties: readonly string[]): void => {
        if (changedProperties.includes('isTouched'))
            this.notifyPropertiesChanged('areAllFieldsTouched');
    };
}