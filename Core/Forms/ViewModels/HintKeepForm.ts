import type { HintKeepFormField } from "./HintKeepFormField";
import type { IHintKeepReadOnlyFormCollection } from "./HintKeepReadOnlyFormCollection";
import { type IReadOnlyObservableCollection, type IObservableCollection, type FormCollection, Form } from "react-model-view-viewmodel";

export class HintKeepForm extends Form {
    public readonly fields: IReadOnlyObservableCollection<HintKeepFormField<unknown>>;
    public readonly sections: IReadOnlyObservableCollection<HintKeepForm>;
    public readonly sectionsCollections: IReadOnlyObservableCollection<IHintKeepReadOnlyFormCollection>;

    public get hasChanges(): boolean {
        return this.fields.some((field) => field.hasChanged) || this.sectionsCollections.some((formCollection) => formCollection.hasChanges);
    }

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

    protected withSectionsCollection(sectionsCollection: IHintKeepReadOnlyFormCollection): IHintKeepReadOnlyFormCollection {
        return super.withFields.apply(this, arguments);
    }

    protected override onFieldChanged(field: HintKeepFormField<unknown>, changedProperties: readonly (keyof HintKeepFormField<unknown>)[]): void {
        if (changedProperties.includes("hasChanged"))
            this.notifyPropertiesChanged("hasChanges");
    }

    protected override onSectionsCollectionChanged(sectionsCollection: IHintKeepReadOnlyFormCollection, changedProperties: readonly (keyof IHintKeepReadOnlyFormCollection)[]): void {
        if (changedProperties.includes("hasChanges"))
            this.notifyPropertiesChanged("hasChanges");
    }
}