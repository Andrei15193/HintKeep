import type { HintKeepFormField } from "./HintKeepFormField";
import { type IReadOnlyObservableCollection, type IReadOnlyFormCollection, type IObservableCollection, type FormCollection, Form } from "react-model-view-viewmodel";

export class HintKeepForm extends Form {
    public readonly fields: IReadOnlyObservableCollection<HintKeepFormField<unknown>>;
    public readonly sections: IReadOnlyObservableCollection<HintKeepForm>;
    public readonly sectionsCollections: IReadOnlyObservableCollection<IReadOnlyFormCollection<HintKeepForm>>;

    public validate(): void {
        this.fields.forEach((field) => {
            field.validation.validate();
            field.wasTouched = true;
        });

        this.sections.forEach((section) => section.validate());
    }

    protected withFields(...fields: readonly HintKeepFormField<any>[]): IObservableCollection<HintKeepFormField<any>> {
        return super.withFields.apply(this, arguments);
    }

    protected withSections(...sections: readonly HintKeepForm[]): FormCollection<HintKeepForm> {
        return super.withFields.apply(this, arguments);
    }

    protected withSectionsCollection(sectionsCollection: IReadOnlyFormCollection<HintKeepForm>): IReadOnlyFormCollection<HintKeepForm> {
        return super.withFields.apply(this, arguments);
    }
}