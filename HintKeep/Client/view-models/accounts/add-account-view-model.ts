import type { AxiosResponse } from 'axios';
import type { IEvent } from 'react-model-view-viewmodel'
import type { IConflictResponseData, IRequestData, IResponseData, IUnprocessableEntityResponseData } from '../../api/accounts/post';
import { DispatchEvent } from 'react-model-view-viewmodel'
import { ApiViewModel } from '../api-view-model';
import { AccountFormViewModel } from './account-form-view-model';

export class AddAccountViewModel extends ApiViewModel {
    private readonly _submittedEvent: DispatchEvent = new DispatchEvent();

    public readonly form: AccountFormViewModel = new AccountFormViewModel();

    public get submittedEvent(): IEvent {
        return this._submittedEvent;
    }

    public async submitAsync(): Promise<void> {
        this.form.fields.forEach(field => field.isTouched = true);
        if (this.form.isValid) {
            await this
                .post<IRequestData>('/api/accounts', {
                    name: this.form.name.value,
                    hint: this.form.hint.value,
                    isPinned: this.form.isPinned.value,
                    notes: this.form.notes.value
                })
                .on(201, (_: AxiosResponse<IResponseData>) => {
                    this._submittedEvent.dispatch(this);
                })
                .on(409, (_: AxiosResponse<IConflictResponseData>) => {
                    this.form.name.error = 'validation.errors.nameNotUnique';
                })
                .on(422, ({ data: { name: nameErrors = [], hint: hintErrors = [] } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                    this.form.name.error = nameErrors[0];
                    this.form.hint.error = hintErrors[0];
                })
                .sendAsync();
        }
    }
}