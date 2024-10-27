import type { AxiosResponse } from 'axios';
import type { INotFoundResponseData, IRequestData, IResponseData, IUnprocessableEntityResponseData } from '../../api/users/confirmations/post';
import { type IEvent, EventDispatcher } from 'react-model-view-viewmodel';
import { ApiViewModel } from '../api-view-model';
import { required } from '../validation';
import { HintKeepForm, HintKeepFormField } from '../forms';

export class ConfirmUserViewModel extends ApiViewModel {
    private readonly _confirmedEvent: EventDispatcher<this> = new EventDispatcher<this>();

    public readonly form: ConfirmUserFormViewModel = new ConfirmUserFormViewModel();

    public get confirmed(): IEvent<this> {
        return this._confirmedEvent;
    }

    public async confirmAsync(): Promise<void> {
        this.form.fields.forEach(field => field.isTouched = true);
        if (this.form.isValid)
            await this
                .post<IRequestData>('/api/users/confirmations', {
                    token: this.form.token.value
                })
                .on(201, (_: AxiosResponse<IResponseData>) => {
                    this._confirmedEvent.dispatch(this);
                })
                .on(404, (_: AxiosResponse<INotFoundResponseData>) => {
                    this.form.token.error = 'errors.confirmation.tokenExpired';
                })
                .on(422, ({ data: { token: tokenError = [] } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                    this.form.token.error = tokenError[0];
                })
                .sendAsync();
    }
}

class ConfirmUserFormViewModel extends HintKeepForm {
    public constructor() {
        super();

        this.withFields(
            this.token = new HintKeepFormField<string>({
                name: 'token',
                initialValue: '',
                validators: [required]
            })
        )
    }

    public readonly token: HintKeepFormField<string>;
}