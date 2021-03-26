import { FieldChangedCallback } from "./form-field-collection-view-model";
import { FormFieldViewModel } from "./form-field-view-model";

export type FieldValidationCallback<TValue> = (field: FormFieldViewModel<TValue>) => readonly string[];

export function combineValidators<TValue>(...validationCallbacks: readonly (FieldValidationCallback<TValue> | undefined)[]): (FieldChangedCallback<TValue> | undefined) {
    const actualValidationCallbacks = validationCallbacks.filter((validationCallback): validationCallback is FieldValidationCallback<TValue> => validationCallback !== undefined);
    switch (actualValidationCallbacks.length) {
        case 0:
            return undefined;

        case 1:
            const onlyValidationCallback = actualValidationCallbacks[0];
            return (field, changedProperties) => {
                if (changedProperties.includes('value'))
                    field.errors = onlyValidationCallback(field);
            };

        default:
            return (field, changedProperties) => {
                if (changedProperties.includes('value')) {
                    const errors: string[] = [];
                    actualValidationCallbacks.forEach(validationCallback => {
                        const validationResults = validationCallback(field);
                        if (validationResults && validationResults.length > 0)
                            validationResults.forEach(validationResult => errors.push(validationResult));
                    });
                    field.errors = errors;
                }
            };
    }
}

const noError: readonly string[] = [];

const requiredError: readonly string[] = ['validation.errors.required'];
export function required(field: FormFieldViewModel<any>): readonly string[] {
    if (field.value === undefined || field.value === null || (typeof field.value === 'string' && field.value.length === 0))
        return requiredError;
    else
        return noError;
}