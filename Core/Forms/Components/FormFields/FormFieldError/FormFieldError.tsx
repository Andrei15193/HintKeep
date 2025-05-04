import { useFormFieldContext } from "../FormField/UseFormFieldContext";

export interface IFormFieldErrorProps {
}

export function FormFieldError(props: IFormFieldErrorProps): React.ReactNode {
    const { field } = useFormFieldContext();

    if (field.wasTouched && field.isInvalid)
        return field.error;
    else
        return null;
}