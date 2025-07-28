import type { HintKeepForm } from "../Forms/ViewModels";

export interface IFormHandler<TForm extends HintKeepForm, TResul> {
    handleAsync(form: TForm): Promise<TResul | null>;
}