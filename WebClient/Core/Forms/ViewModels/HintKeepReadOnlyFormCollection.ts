import type { HintKeepForm } from "./HintKeepForm";
import { type IReadOnlyFormCollection, ReadOnlyFormCollection } from "react-model-view-viewmodel";

export interface IHintKeepReadOnlyFormCollection extends IReadOnlyFormCollection<HintKeepForm> {
    readonly hasChanges: boolean;
}

export class HintKeepReadOnlyFormCollection extends ReadOnlyFormCollection<HintKeepForm> implements IHintKeepReadOnlyFormCollection {
    private readonly _initialLength: number;

    public constructor(sections: Iterable<HintKeepForm>) {
        super(sections);

        this._initialLength = this.length;
    }

    public get hasChanges(): boolean {
        return this._initialLength !== this.length || this.some((form) => form.hasChanges);
    }

    protected override onSectionChanged(section: HintKeepForm, changedProperties: readonly (keyof HintKeepForm)[]): void {
        if (changedProperties.includes("hasChanges"))
            this.notifyPropertiesChanged("hasChanges");
    }
}