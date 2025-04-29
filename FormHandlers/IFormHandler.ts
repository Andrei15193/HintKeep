import type { HintKeepForm } from "../Forms";

export interface IFormHandler<TForm extends HintKeepForm, TResul> {
    handleAsync(form: TForm): Promise<TResul | null>;
}