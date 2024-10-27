import type { HintKeepFormField } from '../../view-models/forms';
import Style from '../style.scss';

export function getValidationClasses(field: HintKeepFormField<unknown>): { [className: string]: boolean } {
    return {
        [Style.isValid]: field.isTouched && field.isValid,
        [Style.isInvalid]: field.isTouched && field.isInvalid
    }
}