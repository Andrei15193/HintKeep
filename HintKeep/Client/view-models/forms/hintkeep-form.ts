import { type IObservableCollection, type IReadOnlyObservableCollection, Form } from "react-model-view-viewmodel";
import { HintKeepFormField } from "./hintkeep-form-field";

export class HintKeepForm extends Form<string> {
    public readonly fields!: IReadOnlyObservableCollection<HintKeepFormField<unknown>>;

    public withFields(...fields: readonly HintKeepFormField<any>[]): IObservableCollection<HintKeepFormField<any>> {
        return super.withFields.apply(this, arguments as any) as  IObservableCollection<HintKeepFormField<any>>;
    }

    public get areAllFieldsTouched(): boolean {
        return this.fields.every(field => field.isTouched);
    }

    protected onFieldChanged(field: HintKeepFormField<unknown>, changedProperties: readonly (keyof HintKeepFormField<unknown>)[]): void {
        super.onFieldChanged.apply(this, arguments as any);

        if (changedProperties.includes('isTouched'))
            this.notifyPropertiesChanged('areAllFieldsTouched');
    }
}