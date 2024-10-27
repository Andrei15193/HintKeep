import { type IFormFieldConfig, FormField } from "react-model-view-viewmodel";

export interface IHintKeepFormFieldConfig<TValue> extends IFormFieldConfig<TValue> {
    readonly isTouched?: boolean;
}

export class HintKeepFormField<TValue> extends FormField<TValue> {
    private _isTouched: boolean;

    public constructor({ isTouched = false, ...config }: IHintKeepFormFieldConfig<TValue>) {
        super(config);

        this._isTouched = isTouched;
    }

    public get isTouched(): boolean {
        return this._isTouched;
    }

    public set isTouched(value: boolean) {
        if (this._isTouched !== value) {
            this._isTouched = value;
            this.notifyPropertiesChanged('isTouched');
        }
    }
}