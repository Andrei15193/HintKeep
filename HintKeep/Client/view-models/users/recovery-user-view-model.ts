import type { AxiosResponse } from "axios";
import type { IEvent } from "react-model-view-viewmodel";
import type { IRequestData as IHintNotificationRequestData, IResponseData as IHintNotificationResponseData, IUnprocessableEntityResponseData as IHintNotificationUnprocessableEntityResponseData } from "../../api/users/hints/notifications/post";
import type { IRequestData as IPasswordResetRequestData, IResponseData as IPasswordResetResponseData, IUnprocessableEntityResponseData as IPasswordResetUnprocessableEntityResponseData } from "../../api/users/passwords/resets/post";
import { DispatchEvent, FormFieldCollectionViewModel, FormFieldViewModel, registerValidators } from "react-model-view-viewmodel";
import { ApiViewModel } from "../api-view-model";
import { required } from "../validation";

export class RecoverUserViewModel extends ApiViewModel {
    private readonly _hintSent: DispatchEvent = new DispatchEvent();
    private readonly _passwordResetRequestSent: DispatchEvent = new DispatchEvent();

    public readonly form: RecoverUserFormViewModel = new RecoverUserFormViewModel();

    public get hintSent(): IEvent {
        return this._hintSent;
    }

    public get passwordResetRequestSent(): IEvent {
        return this._passwordResetRequestSent;
    }

    public async sendHintAsync(): Promise<void> {
        this.form.fields.forEach(field => field.isTouched = true);
        if (this.form.isValid)
            await this
                .post<IHintNotificationRequestData>('/api/users/hints/notifications', {
                    email: this.form.email.value
                })
                .on(201, (_: AxiosResponse<IHintNotificationResponseData>) => {
                    this._hintSent.dispatch(this);
                })
                .on(404, () => {
                    this._hintSent.dispatch(this);
                })
                .on(422, ({ data: { email: emailError } }: AxiosResponse<IHintNotificationUnprocessableEntityResponseData>) => {
                    this.form.email.error = emailError[0];
                })
                .sendAsync();
    }

    public async resetPasswordAsync(): Promise<void> {
        this.form.fields.forEach(field => field.isTouched = true);
        if (this.form.isValid)
            await this
                .post<IPasswordResetRequestData>('/api/users/passwords/resets', {
                    email: this.form.email.value
                })
                .on(201, (_: AxiosResponse<IPasswordResetResponseData>) => {
                    this._passwordResetRequestSent.dispatch(this);
                })
                .on(404, () => {
                    this._passwordResetRequestSent.dispatch(this);
                })
                .on(422, ({ data: { email: emailError = [] } }: AxiosResponse<IPasswordResetUnprocessableEntityResponseData>) => {
                    this.form.email.error = emailError[0];
                })
                .sendAsync();
    }
}

class RecoverUserFormViewModel extends FormFieldCollectionViewModel {
    public constructor() {
        super();
        registerValidators(this.email = this.addField('email', ''), [required]);

        this.fields.forEach(field => field.propertiesChanged.subscribe({ handle: this._fieldChanged }));
    }

    public readonly email: FormFieldViewModel<string>;

    public get areAllFieldsTouched(): boolean {
        return this.fields.every(field => field.isTouched);
    }

    private _fieldChanged = (field: FormFieldViewModel<any>, changedProperties: readonly string[]): void => {
        if (changedProperties.includes('isTouched'))
            this.notifyPropertiesChanged('areAllFieldsTouched');
    };
}