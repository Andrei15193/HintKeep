import { FormField } from 'react-model-view-viewmodel';

export function required(field: FormField<any>): string | undefined {
    if (field.value === undefined || field.value === null || (typeof field.value === 'string' && field.value.length === 0))
        return 'validation.errors.required';
    else
        return undefined;
}