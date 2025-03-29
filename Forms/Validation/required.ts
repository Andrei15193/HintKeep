import type { HintKeepFormField } from "../HintKeepFormField";
import type { IValidator } from "react-model-view-viewmodel";

export class RequiredValidator implements IValidator<HintKeepFormField<unknown>> {
    private static readonly _WhiteSpaceRegex = /^\s*$/;
    private readonly _errorMessage: string;

    public constructor(errorMessage: string = "This field is mandatory. Please fill it in to create an account.") {
        this._errorMessage = errorMessage;
    }

    public onAdd(field: HintKeepFormField<unknown>): void {
        field.isRequired = true;
    }

    public onRemove(field: HintKeepFormField<unknown>): void {
        field.isRequired = false;
    }

    public validate(formField: HintKeepFormField<unknown>): string | null | undefined {
        if (
            formField.value === null
            || formField.value === undefined
            || (typeof formField.value === "string" && RequiredValidator._WhiteSpaceRegex.test(formField.value))
            || (Array.isArray(formField.value) && formField.value.length === 0)
        )
            return this._errorMessage;
        else
            return null;
    }
}