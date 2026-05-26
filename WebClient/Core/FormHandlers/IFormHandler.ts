import type { HintKeepForm } from "../Forms/ViewModels";

export interface IFormHandler<TForm extends HintKeepForm, TResult> {
    handleAsync(form: TForm): Promise<TResult | null>;
}