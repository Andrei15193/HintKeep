import type { HintKeepFormField } from "../HintKeepFormField";
import type { IValidator } from "react-model-view-viewmodel";

export class MaxLengthValidator implements IValidator<HintKeepFormField<string | null | undefined>> {
    private readonly _maxLength: number;
    private readonly _errorMessage: string;

    public constructor(maxLength: 250 | 1000, errorMessage: string = `Your input is too long. Please keep it under ${maxLength} characters.`) {
        this._maxLength = maxLength;
        this._errorMessage = errorMessage;
    }

    public validate(formField: HintKeepFormField<string | null | undefined>): string | null | undefined {
        if (formField.value !== null && formField.value !== undefined && formField.value.length > this._maxLength)
            return this._errorMessage;
        else
            return null;
    }
}