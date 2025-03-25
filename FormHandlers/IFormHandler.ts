import type { HintKeepForm } from "../Forms";

export interface IFormHandler<TForm extends HintKeepForm, TResul = void> {
    handleAsync(form: TForm): Promise<TResul>;
}