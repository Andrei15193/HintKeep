import type { HintKeepFormField } from "../../ViewModels";

export function shouldShowError(field: HintKeepFormField<unknown>): boolean {
    return (field.isInvalid && field.wasTouched);
}