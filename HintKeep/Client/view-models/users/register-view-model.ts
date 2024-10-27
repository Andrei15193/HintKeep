import type { AxiosResponse } from 'axios';
import type { IConflictResponseData, IRequestData, IResponseData, IUnprocessableEntityResponseData } from '../../api/users/post';
import { type IEvent, EventDispatcher } from 'react-model-view-viewmodel';
import { ApiViewModel } from '../api-view-model';
import { required } from '../validation';
import { HintKeepForm, HintKeepFormField } from '../forms';

export class RegisterUserViewModel extends ApiViewModel {
    private readonly _registeredEvent: EventDispatcher<this> = new EventDispatcher<this>();

    public readonly form: RegisterUserFormViewModel = new RegisterUserFormViewModel();

    public get registered(): IEvent<this> {
        return this._registeredEvent;
    }

    public async submitAsync(): Promise<void> {
        this.form.fields.forEach(field => field.isTouched = true);
        if (this.form.isValid)
            await this
                .post<IRequestData>('/api/users', {
                    email: this.form.email.value,
                    hint: this.form.hint.value,
                    password: this.form.password.value
                })
                .on(201, (_: AxiosResponse<IResponseData>) => {
                    this._registeredEvent.dispatch(this);
                })
                .on(409, (_: AxiosResponse<IConflictResponseData>) => {
                    this.form.email.error = 'validation.errors.emailNotUnique';
                })
                .on(422, ({ data: { email: emailErrors = [], hint: hintErrors = [], password: passwordErrors = [] } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                    this.form.email.error = emailErrors[0];
                    this.form.hint.error = hintErrors[0];
                    this.form.password.error = passwordErrors[0];
                })
                .sendAsync();
    }
}

class RegisterUserFormViewModel extends HintKeepForm {
    public constructor() {
        super();

        this.withFields(
            this.email = new HintKeepFormField<string>({
                name: 'email',
                initialValue: '',
                validators: [required]
            }),
            this.hint = new HintKeepFormField<string>({
                name: 'hint',
                initialValue: '',
                validators: [required]
            }),
            this.password = new HintKeepFormField<string>({
                name: 'password',
                initialValue: '',
                validators: [required]
            }),
            this.termsOfServiceAcceptance = new HintKeepFormField<boolean>({
                name: 'termsOfServiceAcceptance',
                initialValue: false
            })
        );
    }

    public readonly email: HintKeepFormField<string>;

    public readonly hint: HintKeepFormField<string>;

    public readonly password: HintKeepFormField<string>;

    public readonly termsOfServiceAcceptance: HintKeepFormField<boolean>;
}