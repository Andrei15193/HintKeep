import type { HintKeepFormField } from "../HintKeepFormField";

const whiteSpaceRegex = /^\s*$/;

export function required(formField: HintKeepFormField<unknown>): string | null {
    if (
        formField.value === null
        || formField.value === undefined
        || (typeof formField.value === "string" && whiteSpaceRegex.test(formField.value))
        || (Array.isArray(formField.value) && formField.value.length === 0)
    )
        return "This field is required";
    else
        return null;
}